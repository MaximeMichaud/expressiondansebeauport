using Domain.Common;

namespace Domain.Entities;

public enum MediaFileType
{
    Image,
    Video,
    Document,
    Other
}

public class MediaFile : AuditableEntity
{
    public string FileName { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public string BlobUrl { get; private set; } = null!;
    public MediaFileType FileType { get; private set; }
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
        FileType = ResolveFileType(contentType);
    }

    public void SetAltText(string? altText) => AltText = altText;

    public void SetDimensions(int? width, int? height)
    {
        Width = width;
        Height = height;
    }

    public void SetBlobUrl(string blobUrl) => BlobUrl = blobUrl;

    private static MediaFileType ResolveFileType(string contentType)
    {
        if (contentType.StartsWith("image/")) return MediaFileType.Image;
        if (contentType.StartsWith("video/")) return MediaFileType.Video;
        if (contentType.StartsWith("audio/")) return MediaFileType.Other;

        return contentType switch
        {
            "application/pdf" => MediaFileType.Document,
            "application/msword" => MediaFileType.Document,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => MediaFileType.Document,
            "application/vnd.ms-excel" => MediaFileType.Document,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => MediaFileType.Document,
            "application/vnd.ms-powerpoint" => MediaFileType.Document,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => MediaFileType.Document,
            "application/vnd.oasis.opendocument.text" => MediaFileType.Document,
            "application/vnd.oasis.opendocument.spreadsheet" => MediaFileType.Document,
            "application/vnd.oasis.opendocument.presentation" => MediaFileType.Document,
            "text/plain" => MediaFileType.Document,
            "text/csv" => MediaFileType.Document,
            _ => MediaFileType.Other
        };
    }
}
