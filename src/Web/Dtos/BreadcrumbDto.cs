namespace Web.Dtos;

public class BreadcrumbDto
{
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public string? AbsoluteUrl { get; set; }
    public bool IsCurrent { get; set; }
}
