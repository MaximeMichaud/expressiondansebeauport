namespace Web.Features.Public.Seo;

public interface ISeoFilesService
{
    string GetRobotsTxt();
    string GetSitemapIndexXml();
    string GetPageSitemapXml();
}
