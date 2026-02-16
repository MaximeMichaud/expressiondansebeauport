namespace Web.Dtos;

public class PageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public bool IsHomePage { get; set; }
    public List<PageSectionDto> Sections { get; set; } = new();
}

public class PageSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string HtmlContent { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}
