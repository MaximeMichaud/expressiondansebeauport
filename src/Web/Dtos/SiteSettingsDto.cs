namespace Web.Dtos;

public class SiteSettingsDto
{
    public Guid Id { get; set; }
    public string SiteTitle { get; set; } = null!;
    public string? Tagline { get; set; }
    public string PrimaryColor { get; set; } = null!;
    public string? SecondaryColor { get; set; }
    public Guid? LogoMediaFileId { get; set; }
    public string? LogoUrl { get; set; }
    public Guid? FaviconMediaFileId { get; set; }
    public string? FaviconUrl { get; set; }
    public string HeadingFont { get; set; } = null!;
    public string BodyFont { get; set; } = null!;
    public string? FooterDescription { get; set; }
    public string? FooterAddress { get; set; }
    public string? FooterCity { get; set; }
    public string? FooterPhone { get; set; }
    public string? FooterEmail { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? CopyrightText { get; set; }
    public bool IsMaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = null!;
    public int MaintenanceRetryAfter { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = [];
    public List<FooterPartnerDto> FooterPartners { get; set; } = [];
}

public class SocialLinkDto
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }
}

public class FooterPartnerDto
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }
    public string? MediaUrl { get; set; }
    public string AltText { get; set; } = null!;
    public string? Url { get; set; }
    public int SortOrder { get; set; }
}
