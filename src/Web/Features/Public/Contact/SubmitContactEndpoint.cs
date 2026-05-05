using Application.Interfaces.Services.Notifications;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web.Extensions;
using Web.Features.Common;

namespace Web.Features.Public.Contact;

public class SubmitContactRequest : ISanitizable
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Honeypot { get; set; }
    public string? RecipientEmail { get; set; }
    public string? BlockId { get; set; }
    public string? PageSlug { get; set; }

    public void Sanitize()
    {
        Name = Name.Trim();
        Email = Email.SanitizeEmailAddress();
        Message = Message.Trim();
        Honeypot = Honeypot?.Trim();
        RecipientEmail = string.IsNullOrWhiteSpace(RecipientEmail) ? null : RecipientEmail.SanitizeEmailAddress();
        BlockId = BlockId?.Trim();
        PageSlug = PageSlug?.Trim();
    }
}

public class SubmitContactValidator : Validator<SubmitContactRequest>
{
    public SubmitContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.RecipientEmail)
            .EmailAddress()
            .MaximumLength(320)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientEmail));

        RuleFor(x => x.BlockId).MaximumLength(100);
        RuleFor(x => x.PageSlug).MaximumLength(200);
    }
}

public class SubmitContactEndpoint : EndpointWithSanitizedRequest<SubmitContactRequest, SucceededOrNotResponse>
{
    private readonly INotificationService _notificationService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SubmitContactEndpoint> _logger;

    public SubmitContactEndpoint(
        INotificationService notificationService,
        ISiteSettingsRepository siteSettingsRepository,
        IHostEnvironment hostEnvironment,
        ILogger<SubmitContactEndpoint> logger)
    {
        _notificationService = notificationService;
        _siteSettingsRepository = siteSettingsRepository;
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

        var settings = await _siteSettingsRepository.Get();
        var destinationEmail = !string.IsNullOrWhiteSpace(req.RecipientEmail)
            ? req.RecipientEmail
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
