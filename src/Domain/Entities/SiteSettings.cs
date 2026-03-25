using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.Entities;

public class SiteSettings : Entity
{
    public string SiteTitle { get; private set; } = "Expression Danse de Beauport";
    public string? Tagline { get; private set; }
    public string PrimaryColor { get; private set; } = "#be1e2c";
    public string? SecondaryColor { get; private set; }
    public Guid? LogoMediaFileId { get; private set; }
    public Guid? FaviconMediaFileId { get; private set; }
    public string HeadingFont { get; private set; } = "Montserrat";
    public string BodyFont { get; private set; } = "Karla";

    public string? FooterDescription { get; private set; }
    public string? FooterAddress { get; private set; }
    public string? FooterCity { get; private set; }
    public string? FooterPhone { get; private set; }
    public string? FooterEmail { get; private set; }
    public string? FacebookUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    public string? CopyrightText { get; private set; }

    public MediaFile? LogoMediaFile { get; private set; }
    public MediaFile? FaviconMediaFile { get; private set; }
    public ICollection<SocialLink> SocialLinks { get; private set; } = new List<SocialLink>();
    public ICollection<FooterPartner> FooterPartners { get; private set; } = new List<FooterPartner>();

    public SiteSettings() { }

    public void SetSiteTitle(string title) => SiteTitle = title;
    public void SetTagline(string? tagline) => Tagline = tagline;

    public void SetPrimaryColor(string color)
    {
        ValidateHexColor(color);
        PrimaryColor = color;
    }

    public void SetSecondaryColor(string? color)
    {
        if (color is not null) ValidateHexColor(color);
        SecondaryColor = color;
    }

    public void SetLogoMediaFileId(Guid? id) => LogoMediaFileId = id;
    public void SetFaviconMediaFileId(Guid? id) => FaviconMediaFileId = id;
    public void SetHeadingFont(string font) => HeadingFont = font;
    public void SetBodyFont(string font) => BodyFont = font;
    public void SetFooterDescription(string? description) => FooterDescription = description;
    public void SetFooterAddress(string? address) => FooterAddress = address;
    public void SetFooterCity(string? city) => FooterCity = city;
    public void SetFooterPhone(string? phone) => FooterPhone = phone;
    public void SetFooterEmail(string? email) => FooterEmail = email;
    public void SetFacebookUrl(string? url) => FacebookUrl = url;
    public void SetInstagramUrl(string? url) => InstagramUrl = url;
    public void SetCopyrightText(string? text) => CopyrightText = text;

    private static void ValidateHexColor(string color)
    {
        if (!Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException($"Invalid hex color format: {color}");
    }
}
