using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Sidio.Sitemap.Core;
using Sidio.Sitemap.Core.Services;

namespace Web.Features.Public.Seo;

public class SeoFilesService : ISeoFilesService
{
    private const string PageSitemapPath = "/page-sitemap.xml";
    private const string SitemapIndexPath = "/sitemap_index.xml";

    private readonly IPageRepository _pageRepository;
    private readonly ISitemapService _sitemapService;
    private readonly ISitemapIndexService _sitemapIndexService;
    private readonly ApplicationSettings _applicationSettings;

    public SeoFilesService(
        IPageRepository pageRepository,
        ISitemapService sitemapService,
        ISitemapIndexService sitemapIndexService,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _pageRepository = pageRepository;
        _sitemapService = sitemapService;
        _sitemapIndexService = sitemapIndexService;
        _applicationSettings = applicationSettings.Value;
    }

    public string GetRobotsTxt()
    {
        var baseUri = GetBaseUri();
        var sitemapUrl = GetAbsoluteUrl(baseUri, SitemapIndexPath);

        return string.Join('\n', new[]
        {
            "User-agent: *",
            "Disallow: /admin/",
            "Disallow: /api/",
            "Disallow: /erreur",
            "Disallow: /maintenance",
            "Disallow: /preview/",
            "Disallow: /social/",
            "",
            $"Sitemap: {sitemapUrl}",
            ""
        });
    }

    public string GetSitemapIndexXml()
    {
        var baseUri = GetBaseUri();
        var pages = _pageRepository.GetPublished();
        var pageSitemapLastModified = pages
            .Select(GetLastModified)
            .Where(date => date.HasValue)
            .Max();

        var sitemapIndex = new SitemapIndex([
            new SitemapIndexNode(GetAbsoluteUrl(baseUri, PageSitemapPath), pageSitemapLastModified)
        ]);

        return _sitemapIndexService.Serialize(sitemapIndex);
    }

    public string GetPageSitemapXml()
    {
        var baseUri = GetBaseUri();
        var nodes = new List<ISitemapNode>
        {
            new SitemapNode(GetAbsoluteUrl(baseUri, "/"))
        };

        nodes.AddRange(_pageRepository.GetPublished().Select(page =>
            new SitemapNode(
                GetAbsoluteUrl(baseUri, $"/{page.Slug}"),
                GetLastModified(page))));

        var sitemap = new Sitemap(nodes);
        return _sitemapService.Serialize(sitemap);
    }

    private Uri GetBaseUri()
    {
        if (!Uri.TryCreate(_applicationSettings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                "Application:BaseUrl doit être configuré avec une URL absolue pour générer les fichiers SEO.");
        }

        var uriBuilder = new UriBuilder(baseUri)
        {
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        return uriBuilder.Uri;
    }

    private static string GetAbsoluteUrl(Uri baseUri, string path)
    {
        if (!path.StartsWith('/'))
            path = $"/{path}";

        return new Uri(baseUri, path).AbsoluteUri;
    }

    private static DateTime? GetLastModified(Page page)
    {
        var lastModified = page.LastModified ?? page.Created;
        return lastModified.ToDateTimeUtc();
    }
}
