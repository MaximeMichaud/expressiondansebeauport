using System.Xml.Linq;
using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using NodaTime;
using Sidio.Sitemap.Core.Serialization;
using Sidio.Sitemap.Core.Services;
using Web.Features.Public.Seo;

namespace Tests.Web.Features.Public.Seo;

public class SeoFilesServiceTests
{
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public void GetRobotsTxt_ReturnsCrawlRulesAndSitemapLocation()
    {
        var service = CreateService();

        var robotsTxt = service.GetRobotsTxt();

        robotsTxt.ShouldContain("User-agent: *");
        robotsTxt.ShouldContain("Disallow: /admin/");
        robotsTxt.ShouldContain("Disallow: /api/");
        robotsTxt.ShouldContain("Disallow: /preview/");
        robotsTxt.ShouldContain("Disallow: /social/");
        robotsTxt.ShouldContain("Sitemap: https://expression.mich.sh/sitemap_index.xml");
    }

    [Fact]
    public void GetSitemapIndexXml_ReturnsPageSitemapLocation()
    {
        var page = CreatePage("Notre école", "notre-ecole", Instant.FromUtc(2026, 4, 2, 10, 30));
        var service = CreateService([page]);

        var document = XDocument.Parse(service.GetSitemapIndexXml());

        var locations = document.Descendants(SitemapNamespace + "loc").Select(node => node.Value).ToList();
        locations.ShouldBe(["https://expression.mich.sh/page-sitemap.xml"]);
        document.Descendants(SitemapNamespace + "lastmod").ShouldHaveSingleItem();
    }

    [Fact]
    public void GetPageSitemapXml_ReturnsHomeAndPublishedPages()
    {
        var service = CreateService([
            CreatePage("Notre école", "notre-ecole", Instant.FromUtc(2026, 4, 2, 10, 30)),
            CreatePage("Camp d'été", "camp-d-ete", Instant.FromUtc(2026, 4, 3, 9, 15))
        ]);

        var document = XDocument.Parse(service.GetPageSitemapXml());

        var locations = document.Descendants(SitemapNamespace + "loc").Select(node => node.Value).ToList();
        locations.ShouldBe([
            "https://expression.mich.sh/",
            "https://expression.mich.sh/notre-ecole",
            "https://expression.mich.sh/camp-d-ete"
        ]);
    }

    [Fact]
    public void GetRobotsTxt_ThrowsWhenBaseUrlIsInvalid()
    {
        var service = CreateService(baseUrl: "expression.mich.sh");

        var exception = Should.Throw<InvalidOperationException>(() => service.GetRobotsTxt());
        exception.Message.ShouldContain("Application:BaseUrl");
    }

    private static SeoFilesService CreateService(List<Page>? pages = null, string baseUrl = "https://expression.mich.sh")
    {
        var pageRepository = new Mock<IPageRepository>();
        pageRepository.Setup(repository => repository.GetPublished()).Returns(pages ?? []);

        var serializer = new XmlSerializer();
        var settings = Options.Create(new ApplicationSettings
        {
            BaseUrl = baseUrl,
            RedirectUrl = baseUrl,
            ErrorNotificationDestination = "tests@example.com"
        });

        return new SeoFilesService(
            pageRepository.Object,
            new SitemapService(serializer),
            new SitemapIndexService(serializer),
            settings);
    }

    private static Page CreatePage(string title, string slug, Instant created)
    {
        var page = new Page(title, slug);
        page.Created = created;
        page.LastModified = created;
        page.Publish();
        return page;
    }
}
