using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.Entities;

public enum HelpCategory
{
    PremiersPas,
    Pages,
    Menus,
    Medias,
    Membres,
    Groupes,
    Sauvegardes,
    Parametres
}

public class HelpArticle : AuditableAndSoftDeletableEntity
{
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public HelpCategory Category { get; private set; }
    public string? Content { get; private set; }
    public string ContentMode { get; private set; } = "html";
    public int SortOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public string? RouteHint { get; private set; }

    public HelpArticle() { }

    public HelpArticle(string title, string slug, HelpCategory category)
    {
        Title = title;
        Slug = GenerateSlug(slug);
        Category = category;
    }

    public void SetTitle(string title) => Title = title;
    public void SetSlug(string slug) => Slug = GenerateSlug(slug);
    public void SetCategory(HelpCategory category) => Category = category;
    public void SetContent(string? content) => Content = content;
    public void SetContentMode(string mode) => ContentMode = mode;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
    public void SetRouteHint(string? routeHint) => RouteHint = routeHint;

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;

    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Normalisation NFD + retrait des marques diacritiques (aligné sur le frontend)
        var normalized = input.ToLowerInvariant().Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var stripped = sb.ToString().Normalize(NormalizationForm.FormC);
        stripped = Regex.Replace(stripped, @"[^a-z0-9]+", "-");
        return stripped.Trim('-');
    }
}
