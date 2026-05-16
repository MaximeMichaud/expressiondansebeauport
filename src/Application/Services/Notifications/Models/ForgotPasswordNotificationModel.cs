namespace Application.Services.Notifications.Models;

public class ForgotPasswordNotificationModel : NotificationModel
{
    public string Link { get; set; }

    public ForgotPasswordNotificationModel(string destination, string locale, string link)
        : base(destination, locale)
    {
        Link = link;
    }

    public override string Subject() => "Réinitialisation de votre mot de passe";

    public override string PlainTextContent() => $@"Bonjour,

Vous avez demandé à réinitialiser votre mot de passe pour votre compte Expression Danse Beauport.

Cliquez sur le lien suivant pour continuer :
{Link}

Si vous n'avez pas demandé cette réinitialisation, vous pouvez ignorer ce courriel.

Expression Danse Beauport";
}
