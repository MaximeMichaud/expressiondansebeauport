using Domain.Common;

namespace Domain.Entities;

public class Review : Entity
{
    public Guid SiteSettingsId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Comment { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public int Rating { get; private set; }
    public int SortOrder { get; private set; }

    public SiteSettings SiteSettings { get; private set; } = null!;

    private Review() { }

    public Review(Guid siteSettingsId, string title, string comment, string author, int rating, int sortOrder)
    {
        SiteSettingsId = siteSettingsId;
        Title = title;
        Comment = comment;
        Author = author;
        SetRating(rating);
        SortOrder = sortOrder;
    }

    public void SetTitle(string title) => Title = title;
    public void SetComment(string comment) => Comment = comment;
    public void SetAuthor(string author) => Author = author;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void SetRating(int rating)
    {
        Rating = Math.Clamp(rating, 1, 5);
    }
}
