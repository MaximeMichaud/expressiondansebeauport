using Application.Interfaces.Mailing;
using Application.Interfaces.Services.Notifications;
using Application.Services.Notifications.Models;
using Domain.Common;
using Domain.Entities.Identity;

namespace Application.Services.Notifications;

public class EmailNotificationService : INotificationService
{
    private const string EMAIL_DEFAULT_CULTURE = "fr";

    private readonly IEmailSender _emailSender;

    public EmailNotificationService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<SucceededOrNotResponse> SendForgotPasswordNotification(User user, string link)
    {
        var model = new ForgotPasswordNotificationModel(
            user.Email!,
            EMAIL_DEFAULT_CULTURE,
            link);

        return await _emailSender.SendAsync(model);
    }

    public async Task<SucceededOrNotResponse> SendTwoFactorAuthenticationCodeNotification(User user,
        string code)
    {
        var model = new TwoFactorAuthenticationNotificationModel(
            user.Email!,
            EMAIL_DEFAULT_CULTURE,
            code);

        return await _emailSender.SendAsync(model);
    }
}