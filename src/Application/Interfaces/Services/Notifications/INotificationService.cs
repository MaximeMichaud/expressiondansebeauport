using Domain.Common;
using Domain.Entities.Identity;

namespace Application.Interfaces.Services.Notifications;

public interface INotificationService
{
    Task<SucceededOrNotResponse> SendForgotPasswordNotification(User user, string link);
    Task<SucceededOrNotResponse> SendTwoFactorAuthenticationCodeNotification(User user, string code);
    Task<SucceededOrNotResponse> SendConfirmationCodeNotification(User user, string code);
    Task<SucceededOrNotResponse> SendContactFormNotification(
        string destinationEmail,
        string senderName,
        string senderEmail,
        string message,
        string? pageSlug = null);
}
