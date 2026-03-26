namespace Web.Features.Social.Messages.Send;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
}
