namespace Application.Services.Notifications.Models;

public class ConfirmationCodeNotificationModel : NotificationModel
{
    public string Code { get; set; }

    public ConfirmationCodeNotificationModel(string destination, string locale, string code)
        : base(destination, locale)
    {
        Code = code;
    }

    public override string Subject() => "Votre code de confirmation";

    public override string PlainTextContent() => $@"Bonjour,

Voici votre code de confirmation pour activer votre compte Expression Danse Beauport :

{Code}

Si vous n'avez pas créé de compte, vous pouvez ignorer ce courriel.

Expression Danse Beauport";
}
