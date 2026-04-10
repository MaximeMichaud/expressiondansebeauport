namespace Web.Features.Social.Messages.Send;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaThumbnailUrl { get; set; }
    public string? MediaOriginalUrl { get; set; }
}
