namespace Web.Features.Social.Messages.GetMessages;

public class GetMessagesRequest
{
    public Guid ConversationId { get; set; }
    public int Page { get; set; } = 1;
}
