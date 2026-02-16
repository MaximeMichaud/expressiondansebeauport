using Domain.Common;

namespace Domain.Entities;

public class Page : AuditableAndSoftDeletableEntity, ISanitizable
{
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public bool IsHomePage { get; private set; }
    public List<PageSection> Sections { get; private set; } = new();

    public Page() {}

    public Page(string title, string slug, bool isHomePage = false)
    {
        Title = title;
        Slug = slug;
        IsHomePage = isHomePage;
    }

    public void SetTitle(string title) => Title = title;
    public void SetSlug(string slug) => Slug = slug;

    public void AddSection(PageSection section) => Sections.Add(section);

    public void SanitizeForSaving()
    {
        Title = Title.Trim();
        Slug = Slug.Trim().ToLower();
        foreach (var section in Sections)
        {
            section.SanitizeForSaving();
        }
    }
}
