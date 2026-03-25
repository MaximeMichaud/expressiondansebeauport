using Domain.Common;

namespace Domain.Entities;

public class FooterPartner : Entity
{
    public Guid SiteSettingsId { get; private set; }
    public Guid MediaFileId { get; private set; }
    public string AltText { get; private set; } = null!;
    public string? Url { get; private set; }
    public int SortOrder { get; private set; }

    public SiteSettings SiteSettings { get; private set; } = null!;
    public MediaFile MediaFile { get; private set; } = null!;

    private FooterPartner() { }

    public FooterPartner(Guid siteSettingsId, Guid mediaFileId, string altText, string? url, int sortOrder)
    {
        SiteSettingsId = siteSettingsId;
        MediaFileId = mediaFileId;
        AltText = altText;
        Url = url;
        SortOrder = sortOrder;
    }

    public void SetMediaFileId(Guid mediaFileId) => MediaFileId = mediaFileId;
    public void SetAltText(string altText) => AltText = altText;
    public void SetUrl(string? url) => Url = url;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
