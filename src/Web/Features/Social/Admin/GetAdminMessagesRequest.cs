namespace Web.Features.Social.Admin;

public class GetAdminMessagesRequest
{
    public Guid ConversationId { get; set; }
    public int Page { get; set; } = 1;
}
