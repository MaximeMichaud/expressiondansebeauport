using Application.Services.Notifications.Models;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Mailing;

public interface ISendGridMessageFactory
{
    SendGridMessage CreateFromModel<TModel>(TModel model) where TModel : NotificationModel;
}