namespace Web.Dtos;

public class MediaFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public string BlobUrl { get; set; } = null!;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
}
