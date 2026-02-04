namespace Application.Services.Notifications.Models;

public class ForgotPasswordNotificationModel : NotificationModel
{
    public string Link { get; set; }
    
    public ForgotPasswordNotificationModel(string destination, string locale, string link) 
        : base(destination, locale)
    {
        Link = link;
    }

    public override string TemplateId()
    {
        if (Locale == "fr")
            return "d-ccea0bf1594048259d41fb52c2c23614";
        return "d-6bceb5f892064a7b95cc03fe16b45943";
    }

    public override object TemplateData()
    {
        return new
        {
            ButtonUrl = Link
        };
    }
}