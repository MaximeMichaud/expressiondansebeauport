using System.Net;

namespace Application.Services.Notifications.Models;

public class ContactFormNotificationModel : NotificationModel
{
    public string SenderName { get; }
    public string SenderEmail { get; }
    public string Message { get; }
    public string? PageSlug { get; }

    public ContactFormNotificationModel(
        string destination,
        string locale,
        string senderName,
        string senderEmail,
        string message,
        string? pageSlug = null)
        : base(destination, locale)
    {
        SenderName = senderName;
        SenderEmail = senderEmail;
        Message = message;
        PageSlug = pageSlug;
    }

    public override string TemplateId() => string.Empty;

    public override object TemplateData() => new { };

    public override string Subject() => $"Nouveau message de contact{(string.IsNullOrWhiteSpace(PageSlug) ? string.Empty : $" - {PageSlug}")}";

    public override string PlainTextContent()
    {
        return
            $"Nom: {SenderName}{Environment.NewLine}" +
            $"Courriel: {SenderEmail}{Environment.NewLine}" +
            $"Page: {PageSlug ?? "-"}{Environment.NewLine}{Environment.NewLine}" +
            "Message:" + Environment.NewLine +
            Message;
    }

    public override string HtmlContent()
    {
        var encodedName = WebUtility.HtmlEncode(SenderName);
        var encodedEmail = WebUtility.HtmlEncode(SenderEmail);
        var encodedPage = WebUtility.HtmlEncode(PageSlug ?? "-");
        var encodedMessage = WebUtility.HtmlEncode(Message).Replace("\n", "<br />");

        return
            "<h2>Nouveau message de contact</h2>" +
            $"<p><strong>Nom :</strong> {encodedName}</p>" +
            $"<p><strong>Courriel :</strong> {encodedEmail}</p>" +
            $"<p><strong>Page :</strong> {encodedPage}</p>" +
            $"<p><strong>Message :</strong><br />{encodedMessage}</p>";
    }
}
