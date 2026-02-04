namespace Application.Services.Notifications.Dtos;

public class AttachmentDto
{
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string BodyStream { get; set; } = null!;
}