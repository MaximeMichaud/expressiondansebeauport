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
}
