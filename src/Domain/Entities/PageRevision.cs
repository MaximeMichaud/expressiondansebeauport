using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public enum RevisionType
{
    Manual,
    Autosave
}

public class PageRevision : Entity
{
    public Guid PageId { get; private set; }
    public Page Page { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Content { get; private set; }
    public string? CustomCss { get; private set; }
    public string ContentMode { get; private set; } = "html";
    public string? Blocks { get; private set; }
    public string? MetaDescription { get; private set; }
    public PageStatus Status { get; private set; }

    public int RevisionNumber { get; private set; }
    public RevisionType RevisionType { get; private set; }
    public Instant CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public PageRevision() { }

    public static PageRevision CreateFromPage(Page page, int revisionNumber, RevisionType type, string? createdBy, Instant createdAt)
    {
        return new PageRevision
        {
            PageId = page.Id,
            Title = page.Title,
            Content = page.Content,
            CustomCss = page.CustomCss,
            ContentMode = page.ContentMode,
            Blocks = page.Blocks,
            MetaDescription = page.MetaDescription,
            Status = page.Status,
            RevisionNumber = revisionNumber,
            RevisionType = type,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    public bool HasSameContentAs(Page page)
    {
        return Title == page.Title
               && Content == page.Content
               && CustomCss == page.CustomCss
               && ContentMode == page.ContentMode
               && Blocks == page.Blocks
               && MetaDescription == page.MetaDescription
               && Status == page.Status;
    }
}
