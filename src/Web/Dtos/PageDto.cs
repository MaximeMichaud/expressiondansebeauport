namespace Web.Dtos;

public class PageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Content { get; set; }
    public string Status { get; set; } = null!;
    public Guid? FeaturedImageId { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaDescription { get; set; }
    public int SortOrder { get; set; }
}
