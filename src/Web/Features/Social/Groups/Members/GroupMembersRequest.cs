namespace Web.Features.Social.Groups.Members;

public class GroupMembersRequest
{
    public Guid GroupId { get; set; }
    public int Page { get; set; } = 1;
}
