namespace Web.Features.Social.Posts.Comments;

public class GetCommentsRequest
{
    public Guid PostId { get; set; }
    public int Page { get; set; } = 1;
}
