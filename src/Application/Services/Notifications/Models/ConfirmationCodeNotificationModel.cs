namespace Application.Services.Notifications.Models;

public class ConfirmationCodeNotificationModel : NotificationModel
{
    public string Code { get; set; }

    public ConfirmationCodeNotificationModel(string destination, string locale, string code)
        : base(destination, locale)
    {
        Code = code;
    }

    public override string TemplateId()
    {
        // Reuses 2FA template for now — both send a 6-digit code
        // TODO: Create a dedicated SendGrid template for email confirmation
        if (Locale == "fr")
            return "d-b6b894b660614a289e1d3e0e1cc81c17";
        return "d-0ba3cba8d527436dbebe4ee440104d77";
    }

    public override object TemplateData()
    {
        return new
        {
            Code
        };
    }
}
