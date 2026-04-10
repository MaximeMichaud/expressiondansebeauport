namespace Web.Features.Social.Posts.Create;

public class CreatePostRequest
{
    public Guid GroupId { get; set; }
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<CreatePostMediaItem> Media { get; set; } = new();
}

public class CreatePostMediaItem
{
    public string DisplayUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}
