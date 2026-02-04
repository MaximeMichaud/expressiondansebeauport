using Application.Services.Notifications.Models;
using Domain.Common;

namespace Application.Interfaces.Mailing;

public interface IEmailSender
{
    Task<SucceededOrNotResponse> SendAsync<TModel>(TModel model) where TModel : NotificationModel;
}