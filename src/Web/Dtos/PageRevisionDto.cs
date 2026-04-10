namespace Web.Dtos;

public class PageRevisionDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? CustomCss { get; set; }
    public string ContentMode { get; set; } = "html";
    public string? Blocks { get; set; }
    public string? MetaDescription { get; set; }
    public string Status { get; set; } = null!;
    public int RevisionNumber { get; set; }
    public string RevisionType { get; set; } = null!;
    public string? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class PageRevisionListItemDto
{
    public Guid Id { get; set; }
    public int RevisionNumber { get; set; }
    public string RevisionType { get; set; } = null!;
    public string? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string Title { get; set; } = null!;
}
