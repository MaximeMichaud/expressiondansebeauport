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

    public ResendCodeEndpoint(
        IEmailConfirmationService confirmationService,
        INotificationService notificationService,
        UserManager<User> userManager)
    {
        _confirmationService = confirmationService;
        _notificationService = notificationService;
        _userManager = userManager;
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
        try { await _notificationService.SendConfirmationCodeNotification(user, code); } catch { /* SendGrid not configured */ }
        await HttpContext.Response.WriteAsJsonAsync(new { Succeeded = true, ConfirmationCode = code }, ct);
    }
}
