namespace Web.Features.Public.Seo;

public static class SeoEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapSeoFiles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/robots.txt", (ISeoFilesService seoFilesService, HttpContext context) =>
        {
            SetCacheHeaders(context);
            return Results.Text(seoFilesService.GetRobotsTxt(), "text/plain; charset=utf-8");
        }).AllowAnonymous();

        app.MapGet("/sitemap.xml", (ISeoFilesService seoFilesService, HttpContext context) =>
        {
            SetCacheHeaders(context);
            return Results.Text(seoFilesService.GetSitemapIndexXml(), "application/xml; charset=utf-8");
        }).AllowAnonymous();

        app.MapGet("/sitemap_index.xml", (ISeoFilesService seoFilesService, HttpContext context) =>
        {
            SetCacheHeaders(context);
            return Results.Text(seoFilesService.GetSitemapIndexXml(), "application/xml; charset=utf-8");
        }).AllowAnonymous();

        app.MapGet("/page-sitemap.xml", (ISeoFilesService seoFilesService, HttpContext context) =>
        {
            SetCacheHeaders(context);
            return Results.Text(seoFilesService.GetPageSitemapXml(), "application/xml; charset=utf-8");
        }).AllowAnonymous();

        return app;
    }

    private static void SetCacheHeaders(HttpContext context)
    {
        context.Response.Headers.CacheControl = "public, max-age=3600";
    }
}
