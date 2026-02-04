namespace Application.Services.Notifications.Models;

public class TwoFactorAuthenticationNotificationModel : NotificationModel
{
    public string Code { get; set; }
    
    public TwoFactorAuthenticationNotificationModel(string destination, string locale, string code) 
        : base(destination, locale)
    {
        Code = code;
    }

    public override string TemplateId()
    {
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