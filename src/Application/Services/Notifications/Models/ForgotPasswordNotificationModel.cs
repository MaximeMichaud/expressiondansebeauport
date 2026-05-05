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

    public override string HtmlContent() => $@"<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, Helvetica, sans-serif; color: #1a1a1a; line-height: 1.5; max-width: 560px; margin: 0 auto; padding: 24px;"">
  <h2 style=""margin: 0 0 16px;"">Réinitialisation du mot de passe</h2>
  <p>Bonjour,</p>
  <p>Vous avez demandé à réinitialiser votre mot de passe pour votre compte Expression Danse Beauport. Cliquez sur le bouton ci-dessous pour continuer&nbsp;:</p>
  <p style=""margin: 24px 0;"">
    <a href=""{Link}"" style=""display: inline-block; padding: 12px 24px; background: #1a1a1a; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: 600;"">Réinitialiser mon mot de passe</a>
  </p>
  <p style=""font-size: 13px; color: #6b7280;"">Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur&nbsp;:<br><a href=""{Link}"" style=""color: #1a1a1a; word-break: break-all;"">{Link}</a></p>
  <p style=""font-size: 13px; color: #6b7280;"">Si vous n'avez pas demandé cette réinitialisation, vous pouvez ignorer ce courriel.</p>
  <p style=""margin-top: 32px;"">— Expression Danse Beauport</p>
</body>
</html>";
}
