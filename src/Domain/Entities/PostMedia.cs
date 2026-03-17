using Domain.Common;

namespace Domain.Entities;

public class PostMedia : Entity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public string MediaUrl { get; private set; } = null!;
    public string? ThumbnailUrl { get; private set; }
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public int SortOrder { get; private set; }

    public void SetPost(Post post)
    {
        Post = post;
        PostId = post.Id;
    }

    public void SetMediaUrl(string url) => MediaUrl = url;
    public void SetThumbnailUrl(string? url) => ThumbnailUrl = url;
    public void SetContentType(string contentType) => ContentType = contentType;
    public void SetSize(long size) => Size = size;
    public void SetSortOrder(int order) => SortOrder = order;
}
