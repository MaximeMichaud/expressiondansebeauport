using Web.Features.Common;

namespace Web.Features.Admins.Pages.UpdatePage;

public class UpdatePageRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public List<UpdatePageSectionRequest> Sections { get; set; } = new();

    public void Sanitize()
    {
        Title = Title.Trim();
        foreach (var section in Sections)
        {
            section.Title = section.Title.Trim();
        }
    }
}

public class UpdatePageSectionRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = null!;
    public string HtmlContent { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}
