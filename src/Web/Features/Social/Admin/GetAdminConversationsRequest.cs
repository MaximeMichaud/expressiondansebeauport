namespace Web.Features.Social.Admin;

public class GetAdminConversationsRequest
{
    public Guid MemberId { get; set; }
    public int Page { get; set; } = 1;
}
