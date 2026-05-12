using Ganss.Xss;

namespace Web.Services;

public interface IHelpHtmlSanitizer
{
    string? Sanitize(string? html);
}

public class HelpHtmlSanitizer : IHelpHtmlSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public HelpHtmlSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "h1", "h2", "h3", "h4", "h5", "h6",
            "p", "br", "hr",
            "ul", "ol", "li",
            "strong", "em", "b", "i", "u", "s",
            "blockquote", "pre", "code",
            "a", "img",
            "table", "thead", "tbody", "tr", "th", "td"
        })
            _sanitizer.AllowedTags.Add(tag);

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("src");
        _sanitizer.AllowedAttributes.Add("alt");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.AllowedAttributes.Add("target");
        _sanitizer.AllowedAttributes.Add("rel");

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        _sanitizer.AllowDataAttributes = false;
    }

    public string? Sanitize(string? html)
    {
        if (html is null) return null;
        if (html.Length == 0) return string.Empty;
        return _sanitizer.Sanitize(html);
    }
}
