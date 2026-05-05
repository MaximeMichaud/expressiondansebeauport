namespace Web.Features.Social.Groups.JoinRequests.Create;

public class CreateJoinRequestRequest
{
    public Guid GroupId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
