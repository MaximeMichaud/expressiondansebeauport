using Application.Helpers;
using Application.Services.Notifications.Dtos;

namespace Application.Services.Notifications.Models;

public abstract class NotificationModel
{
    public string Destination { get; }
    public string Locale { get; }
    public List<AttachmentDto> Attachments { get; }

    protected NotificationModel(string destination, string locale, List<AttachmentDto>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Recipient address must be set.", nameof(destination));

        Destination = destination;
        Locale = CultureHelper.FormatTwoLetterCulture(locale);

        Attachments = attachments ?? new List<AttachmentDto>();
    }

    public abstract string TemplateId();
    public abstract object TemplateData();
}