using Domain.Common;

namespace Domain.Entities;

public class MessageMedia : Entity
{
    public Guid MessageId { get; private set; }
    public Message Message { get; private set; } = null!;
    public string MediaUrl { get; private set; } = null!;
    public string? ThumbnailUrl { get; private set; }
    public string? OriginalUrl { get; private set; }
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public int SortOrder { get; private set; }

    public void SetMessage(Message message)
    {
        Message = message;
        MessageId = message.Id;
    }

    public void SetMediaUrl(string url) => MediaUrl = url;
    public void SetThumbnailUrl(string? url) => ThumbnailUrl = url;
    public void SetOriginalUrl(string? url) => OriginalUrl = url;
    public void SetContentType(string contentType) => ContentType = contentType;
    public void SetSize(long size) => Size = size;
    public void SetSortOrder(int order) => SortOrder = order;
}
