using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Web.Dtos;

namespace Web.Features.Public.Breadcrumbs;

public class BreadcrumbService : IBreadcrumbService
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly ApplicationSettings _settings;

    public BreadcrumbService(
        INavigationMenuRepository menuRepository,
        IOptions<ApplicationSettings> settings)
    {
        _menuRepository = menuRepository;
        _settings = settings.Value;
    }

    public IReadOnlyList<BreadcrumbDto> GetForPage(Page page)
    {
        var baseUri = GetBaseUri();
        var breadcrumbs = new List<BreadcrumbDto>
        {
            CreateBreadcrumb("Accueil", "/", baseUri, isCurrent: false)
        };

        var menu = _menuRepository.FindByLocation(MenuLocation.Primary);
        var menuPath = menu is null
            ? []
            : FindMenuPath(menu.MenuItems, page);

        foreach (var item in menuPath)
        {
            var isCurrent = ItemTargetsPage(item, page);
            var url = isCurrent ? $"/{page.Slug}" : GetItemUrl(item, page);
            var label = isCurrent ? page.Title : item.Label;

            breadcrumbs.Add(CreateBreadcrumb(label, url, baseUri, isCurrent));
        }

        if (!breadcrumbs.Any(item => item.IsCurrent))
            breadcrumbs.Add(CreateBreadcrumb(page.Title, $"/{page.Slug}", baseUri, isCurrent: true));

        return breadcrumbs;
    }

    private static List<NavigationMenuItem> FindMenuPath(
        IEnumerable<NavigationMenuItem> menuItems,
        Page page)
    {
        var orderedItems = menuItems
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Label)
            .ToList();

        var childrenByParent = orderedItems.ToLookup(item => item.ParentId);
        foreach (var root in childrenByParent[null])
        {
            var path = FindMenuPath(root, childrenByParent, page, []);
            if (path is not null)
                return path;
        }

        foreach (var item in orderedItems)
        {
            var path = FindMenuPath(item, childrenByParent, page, []);
            if (path is not null)
                return path;
        }

        return [];
    }

    private static List<NavigationMenuItem>? FindMenuPath(
        NavigationMenuItem item,
        ILookup<Guid?, NavigationMenuItem> childrenByParent,
        Page page,
        IReadOnlyCollection<Guid> visited)
    {
        if (!visited.Contains(item.Id) && ItemTargetsPage(item, page))
            return [item];

        if (visited.Contains(item.Id))
            return null;

        var nextVisited = visited.Append(item.Id).ToHashSet();
        foreach (var child in childrenByParent[item.Id])
        {
            var childPath = FindMenuPath(child, childrenByParent, page, nextVisited);
            if (childPath is null)
                continue;

            return [item, ..childPath];
        }

        return null;
    }

    private static bool ItemTargetsPage(NavigationMenuItem item, Page page)
    {
        if (item.PageId == page.Id || item.Page?.Slug == page.Slug)
            return true;

        return NormalizeInternalPath(item.Url) == $"/{page.Slug}";
    }

    private static string? GetItemUrl(NavigationMenuItem item, Page currentPage)
    {
        if (item.Page?.Slug is not null)
            return $"/{item.Page.Slug}";

        if (item.PageId == currentPage.Id)
            return $"/{currentPage.Slug}";

        return string.IsNullOrWhiteSpace(item.Url) ? null : item.Url;
    }

    private static BreadcrumbDto CreateBreadcrumb(
        string label,
        string? url,
        Uri baseUri,
        bool isCurrent)
    {
        return new BreadcrumbDto
        {
            Label = label,
            Url = url,
            AbsoluteUrl = GetAbsoluteUrl(baseUri, url),
            IsCurrent = isCurrent
        };
    }

    private Uri GetBaseUri()
    {
        if (!Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var baseUri))
            throw new InvalidOperationException("Application:BaseUrl doit contenir une URL absolue valide.");

        var uriBuilder = new UriBuilder(baseUri)
        {
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        return uriBuilder.Uri;
    }

    private static string? GetAbsoluteUrl(Uri baseUri, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (url.StartsWith('/'))
            return new Uri(baseUri, url).AbsoluteUri;

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps
                ? absoluteUri.AbsoluteUri
                : null;

        url = $"/{url}";
        return new Uri(baseUri, url).AbsoluteUri;
    }

    private static string? NormalizeInternalPath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var path = url.Trim();
        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
            path = absoluteUri.AbsolutePath;

        if (!path.StartsWith('/'))
            path = $"/{path}";

        var queryStart = path.IndexOfAny(['?', '#']);
        if (queryStart >= 0)
            path = path[..queryStart];

        path = path.TrimEnd('/');
        return path.Length == 0 ? "/" : path;
    }
}
