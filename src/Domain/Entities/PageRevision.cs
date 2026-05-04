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
        return CreateFromData(page.Id, page.Title, page.Content, page.CustomCss, page.ContentMode,
            page.Blocks, page.MetaDescription, page.Status, revisionNumber, type, createdBy, createdAt);
    }

    public static PageRevision CreateFromData(Guid pageId, string title, string? content, string? customCss,
        string contentMode, string? blocks, string? metaDescription, PageStatus status,
        int revisionNumber, RevisionType type, string? createdBy, Instant createdAt)
    {
        return new PageRevision
        {
            PageId = pageId,
            Title = title,
            Content = content,
            CustomCss = customCss,
            ContentMode = contentMode,
            Blocks = blocks,
            MetaDescription = metaDescription,
            Status = status,
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
