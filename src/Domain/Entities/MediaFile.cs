using Domain.Common;

namespace Domain.Entities;

public class MediaFile : AuditableEntity
{
    public string FileName { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public string BlobUrl { get; private set; } = null!;
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string? AltText { get; private set; }

    public MediaFile() { }

    public MediaFile(string fileName, string originalFileName, string contentType, long size, string blobUrl)
    {
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Size = size;
        BlobUrl = blobUrl;
    }

    public void SetAltText(string? altText) => AltText = altText;

    public void SetDimensions(int? width, int? height)
    {
        Width = width;
        Height = height;
    }

    public void SetBlobUrl(string blobUrl) => BlobUrl = blobUrl;
}
