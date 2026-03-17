namespace Web.Features.Social.Posts.GetFeed;

public class GetFeedRequest
{
    public Guid GroupId { get; set; }
    public int Page { get; set; } = 1;
}
