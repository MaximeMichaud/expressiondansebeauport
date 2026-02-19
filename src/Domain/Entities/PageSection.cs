using Domain.Common;

namespace Domain.Entities;

public class PageSection : AuditableAndSoftDeletableEntity, ISanitizable
{
    public Guid PageId { get; private set; }
    public string Title { get; private set; } = null!;
    public string HtmlContent { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public int SortOrder { get; private set; }

    public Page Page { get; private set; } = null!;

    public PageSection() {}

    public PageSection(string title, string htmlContent, string? imageUrl = null, int sortOrder = 0)
    {
        Title = title;
        HtmlContent = htmlContent;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
    }

    public void SetTitle(string title) => Title = title;
    public void SetHtmlContent(string htmlContent) => HtmlContent = htmlContent;
    public void SetImageUrl(string? imageUrl) => ImageUrl = imageUrl;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
    public void SetPageId(Guid pageId) => PageId = pageId;

    public void SanitizeForSaving()
    {
        Title = Title.Trim();
    }
}
