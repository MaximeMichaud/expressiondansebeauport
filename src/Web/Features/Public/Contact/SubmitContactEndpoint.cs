using System.Text.Json;
using Application.Interfaces.Services.Notifications;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web.Extensions;
using Web.Features.Common;
using PageStatus = Domain.Entities.PageStatus;

namespace Web.Features.Public.Contact;

public class SubmitContactRequest : ISanitizable
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Honeypot { get; set; }
    public string? BlockId { get; set; }
    public string? PageSlug { get; set; }

    public void Sanitize()
    {
        Name = (Name ?? string.Empty).Trim();
        Email = (Email ?? string.Empty).SanitizeEmailAddress();
        Message = (Message ?? string.Empty).Trim();
        Honeypot = Honeypot?.Trim();
        BlockId = BlockId?.Trim();
        PageSlug = PageSlug?.Trim();
    }
}

public class SubmitContactValidator : Validator<SubmitContactRequest>
{
    public SubmitContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("NameRequired")
            .MaximumLength(100).WithErrorCode("NameTooLong");

        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("EmailRequired")
            .EmailAddress().WithErrorCode("EmailInvalid")
            .MaximumLength(320).WithErrorCode("EmailTooLong");

        RuleFor(x => x.Message)
            .NotEmpty().WithErrorCode("MessageRequired")
            .MaximumLength(2000).WithErrorCode("MessageTooLong");

        RuleFor(x => x.BlockId).MaximumLength(100);
        RuleFor(x => x.PageSlug).MaximumLength(200);
    }
}

public class SubmitContactEndpoint : EndpointWithSanitizedRequest<SubmitContactRequest, SucceededOrNotResponse>
{
    private readonly INotificationService _notificationService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly IPageRepository _pageRepository;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SubmitContactEndpoint> _logger;

    public SubmitContactEndpoint(
        INotificationService notificationService,
        ISiteSettingsRepository siteSettingsRepository,
        IPageRepository pageRepository,
        IHostEnvironment hostEnvironment,
        ILogger<SubmitContactEndpoint> logger)
    {
        _notificationService = notificationService;
        _siteSettingsRepository = siteSettingsRepository;
        _pageRepository = pageRepository;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("contact");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SubmitContactRequest req, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(req.Honeypot))
        {
            _logger.LogInformation("Blocked contact form submission by honeypot on page {PageSlug}.", req.PageSlug);
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
            return;
        }

        var blockRecipient = ResolveRecipientFromBlock(req.PageSlug, req.BlockId);
        if (blockRecipient is null)
        {
            _logger.LogWarning(
                "Contact form submission rejected: no published contact-form block found. Page {PageSlug}, block {BlockId}, sender {SenderEmail}.",
                req.PageSlug,
                req.BlockId,
                req.Email);

            await Send.OkAsync(
                new SucceededOrNotResponse(false, new Error("ContactUnavailable", "Le formulaire de contact est indisponible pour le moment.")),
                ct);
            return;
        }

        var settings = await _siteSettingsRepository.Get();
        var destinationEmail = !string.IsNullOrWhiteSpace(blockRecipient)
            ? blockRecipient
            : settings.FooterEmail;

        if (string.IsNullOrWhiteSpace(destinationEmail))
        {
            _logger.LogWarning(
                "Contact form submission received without configured recipient. Page {PageSlug}, block {BlockId}, sender {SenderEmail}.",
                req.PageSlug,
                req.BlockId,
                req.Email);

            if (TryHandleDevelopmentFallback(
                    req,
                    "Contact form accepted in development without configured recipient."))
            {
                await Send.OkAsync(new SucceededOrNotResponse(true), ct);
                return;
            }

            await Send.OkAsync(
                new SucceededOrNotResponse(false, new Error("ContactUnavailable", "Le formulaire de contact est indisponible pour le moment.")),
                ct);
            return;
        }

        SucceededOrNotResponse response;
        try
        {
            response = await _notificationService.SendContactFormNotification(
                destinationEmail,
                req.Name,
                req.Email,
                req.Message,
                req.PageSlug);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Unexpected error while sending contact form email. Page {PageSlug}, block {BlockId}, sender {SenderEmail}.",
                req.PageSlug,
                req.BlockId,
                req.Email);

            if (TryHandleDevelopmentFallback(
                    req,
                    "Contact form accepted in development after mailing exception."))
            {
                await Send.OkAsync(new SucceededOrNotResponse(true), ct);
                return;
            }

            await Send.OkAsync(
                new SucceededOrNotResponse(false, new Error("ContactUnavailable", "Le formulaire de contact est indisponible pour le moment.")),
                ct);
            return;
        }

        if (!response.Succeeded)
        {
            _logger.LogWarning(
                "Failed to send contact form email. Page {PageSlug}, block {BlockId}, sender {SenderEmail}.",
                req.PageSlug,
                req.BlockId,
                req.Email);

            if (TryHandleDevelopmentFallback(
                    req,
                    "Contact form accepted in development after mailing failure response."))
            {
                await Send.OkAsync(new SucceededOrNotResponse(true), ct);
                return;
            }

            await Send.OkAsync(
                new SucceededOrNotResponse(false, new Error("ContactUnavailable", "Le formulaire de contact est indisponible pour le moment.")),
                ct);
            return;
        }

        _logger.LogInformation(
            "Contact form submitted successfully. Page {PageSlug}, block {BlockId}, sender {SenderEmail}.",
            req.PageSlug,
            req.BlockId,
            req.Email);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }

    /// <summary>
    /// Resolves the configured recipient email for a contact-form block.
    /// Returns null when the page or block is missing, not published, not a contact-form, or disabled.
    /// Returns an empty string when the block exists but has no per-block recipient (caller falls back to site settings).
    /// </summary>
    private string? ResolveRecipientFromBlock(string? pageSlug, string? blockId)
    {
        if (string.IsNullOrWhiteSpace(pageSlug) || string.IsNullOrWhiteSpace(blockId))
            return null;

        var page = _pageRepository.FindBySlug(pageSlug);
        if (page is null || page.Status != PageStatus.Published || string.IsNullOrWhiteSpace(page.Blocks))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(page.Blocks);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;

            foreach (var block in doc.RootElement.EnumerateArray())
            {
                if (block.ValueKind != JsonValueKind.Object) continue;
                if (!block.TryGetProperty("id", out var idEl) || idEl.GetString() != blockId) continue;
                if (!block.TryGetProperty("type", out var typeEl) || typeEl.GetString() != "contact-form") return null;
                if (!block.TryGetProperty("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Object) return null;

                var enabled = !dataEl.TryGetProperty("enabled", out var enabledEl)
                              || enabledEl.ValueKind != JsonValueKind.False;
                if (!enabled) return null;

                if (dataEl.TryGetProperty("recipientEmail", out var recipientEl)
                    && recipientEl.ValueKind == JsonValueKind.String)
                    return recipientEl.GetString() ?? string.Empty;

                return string.Empty;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private bool TryHandleDevelopmentFallback(SubmitContactRequest req, string reason)
    {
        if (!_hostEnvironment.IsDevelopment())
            return false;

        _logger.LogInformation(
            "{Reason} Page {PageSlug}, block {BlockId}, sender {SenderName} <{SenderEmail}>, message: {Message}.",
            reason,
            req.PageSlug,
            req.BlockId,
            req.Name,
            req.Email,
            req.Message);

        return true;
    }
}
