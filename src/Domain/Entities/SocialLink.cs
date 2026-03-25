using Domain.Common;

namespace Domain.Entities;

public class SocialLink : Entity
{
    public Guid SiteSettingsId { get; private set; }
    public string Platform { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public int SortOrder { get; private set; }

    public SiteSettings SiteSettings { get; private set; } = null!;

    private SocialLink() { }

    public SocialLink(Guid siteSettingsId, string platform, string url, int sortOrder)
    {
        SiteSettingsId = siteSettingsId;
        Platform = platform;
        Url = url;
        SortOrder = sortOrder;
    }

    public void SetPlatform(string platform) => Platform = platform;
    public void SetUrl(string url) => Url = url;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
