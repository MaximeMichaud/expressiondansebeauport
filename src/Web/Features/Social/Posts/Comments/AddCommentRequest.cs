namespace Web.Features.Social.Posts.Comments;

public class AddCommentRequest
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = null!;
}
