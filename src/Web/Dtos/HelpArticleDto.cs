namespace Web.Dtos;

public class HelpArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Content { get; set; }
    public string ContentMode { get; set; } = "html";
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public string? RouteHint { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
