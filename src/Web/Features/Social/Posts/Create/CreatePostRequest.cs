namespace Web.Features.Social.Posts.Create;

public class CreatePostRequest
{
    public Guid GroupId { get; set; }
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!;
}
