using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.Entities;

public enum PageStatus
{
    Draft,
    Published
}

public class Page : AuditableAndSoftDeletableEntity
{
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Content { get; private set; }
    public PageStatus Status { get; private set; } = PageStatus.Draft;
    public Guid? FeaturedImageId { get; private set; }
    public string? MetaDescription { get; private set; }
    public int SortOrder { get; private set; }

    public MediaFile? FeaturedImage { get; private set; }

    public Page() { }

    public Page(string title, string slug)
    {
        Title = title;
        Slug = GenerateSlug(slug);
    }

    public void SetTitle(string title) => Title = title;
    public void SetSlug(string slug) => Slug = GenerateSlug(slug);
    public void SetContent(string? content) => Content = content;
    public void SetFeaturedImageId(Guid? id) => FeaturedImageId = id;
    public void SetMetaDescription(string? description) => MetaDescription = description;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void Publish() => Status = PageStatus.Published;
    public void SetToDraft() => Status = PageStatus.Draft;

    public static string GenerateSlug(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
