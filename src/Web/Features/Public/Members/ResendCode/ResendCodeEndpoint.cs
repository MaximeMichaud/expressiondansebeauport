using Application.Interfaces.Services.Notifications;
using Application.Services.Members;
using Domain.Common;
using Domain.Entities.Identity;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Web.Features.Public.Members.ResendCode;

public class ResendCodeEndpoint : Endpoint<ResendCodeRequest, SucceededOrNotResponse>
{
    private readonly IEmailConfirmationService _confirmationService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResendCodeEndpoint> _logger;

    public ResendCodeEndpoint(
        IEmailConfirmationService confirmationService,
        INotificationService notificationService,
        UserManager<User> userManager,
        ILogger<ResendCodeEndpoint> logger)
    {
        _confirmationService = confirmationService;
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("members/resend-code");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResendCodeRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email.ToLowerInvariant());
        if (user == null)
        {
            // Don't reveal if email exists
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
            return;
        }

        var code = await _confirmationService.ResendCode(user.Id);
        try
        {
            var sendResult = await _notificationService.SendConfirmationCodeNotification(user, code);
            if (!sendResult.Succeeded)
                _logger.LogError("SendGrid rejected confirmation email for {Email}: {@Errors}", req.Email, sendResult.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email to {Email}.", req.Email);
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
