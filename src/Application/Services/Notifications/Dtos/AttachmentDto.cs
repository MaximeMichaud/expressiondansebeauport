namespace Application.Services.Notifications.Dtos;

public class AttachmentDto
{
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string BodyStream { get; set; } = default!;
}