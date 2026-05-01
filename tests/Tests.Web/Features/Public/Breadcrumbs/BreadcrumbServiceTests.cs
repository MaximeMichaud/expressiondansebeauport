using Application.Settings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Web.Features.Public.Breadcrumbs;

namespace Tests.Web.Features.Public.Breadcrumbs;

public class BreadcrumbServiceTests
{
    [Fact]
    public void GetForPage_ReturnsHomeParentAndCurrentPageFromPrimaryMenu()
    {
        var page = CreatePublishedPage("Camp d'été", "camp-d-ete");
        var parentId = Guid.NewGuid();
        var menu = CreateMenu([
            CreateMenuItem("Camps", sortOrder: 1, id: parentId),
            CreateMenuItem("Camp d'été", sortOrder: 1, parentId: parentId, pageId: page.Id)
        ]);
        var service = CreateService(menu);

        var breadcrumbs = service.GetForPage(page).ToList();

        breadcrumbs.Select(item => item.Label).ShouldBe(["Accueil", "Camps", "Camp d'été"]);
        breadcrumbs.Select(item => item.Url).ShouldBe(["/", null, "/camp-d-ete"]);
        breadcrumbs.Last().IsCurrent.ShouldBeTrue();
        breadcrumbs.Last().AbsoluteUrl.ShouldBe("https://expression.mich.sh/camp-d-ete");
    }

    [Fact]
    public void GetForPage_FallsBackToHomeAndCurrentPageWhenPageIsNotInMenu()
    {
        var page = CreatePublishedPage("Horaire", "horaire");
        var service = CreateService(CreateMenu([]));

        var breadcrumbs = service.GetForPage(page).ToList();

        breadcrumbs.Select(item => item.Label).ShouldBe(["Accueil", "Horaire"]);
        breadcrumbs.Select(item => item.Url).ShouldBe(["/", "/horaire"]);
        breadcrumbs.Last().IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public void GetForPage_UsesMenuUrlWhenPageIdIsNotAvailable()
    {
        var page = CreatePublishedPage("Notre école", "notre-ecole");
        var menu = CreateMenu([
            CreateMenuItem("École", sortOrder: 1, url: "/notre-ecole")
        ]);
        var service = CreateService(menu);

        var breadcrumbs = service.GetForPage(page).ToList();

        breadcrumbs.Select(item => item.Label).ShouldBe(["Accueil", "Notre école"]);
        breadcrumbs.Last().Url.ShouldBe("/notre-ecole");
        breadcrumbs.Last().AbsoluteUrl.ShouldBe("https://expression.mich.sh/notre-ecole");
    }

    [Fact]
    public void GetForPage_ThrowsWhenBaseUrlIsInvalid()
    {
        var page = CreatePublishedPage("Notre école", "notre-ecole");
        var service = CreateService(CreateMenu([]), baseUrl: "expression.mich.sh");

        var exception = Should.Throw<InvalidOperationException>(() => service.GetForPage(page));
        exception.Message.ShouldContain("Application:BaseUrl");
    }

    private static BreadcrumbService CreateService(NavigationMenu? menu, string baseUrl = "https://expression.mich.sh")
    {
        var menuRepository = new Mock<INavigationMenuRepository>();
        menuRepository
            .Setup(repository => repository.FindByLocation(MenuLocation.Primary, true))
            .Returns(menu);

        var settings = Options.Create(new ApplicationSettings
        {
            BaseUrl = baseUrl,
            RedirectUrl = baseUrl,
            ErrorNotificationDestination = "tests@example.com"
        });

        return new BreadcrumbService(menuRepository.Object, settings);
    }

    private static Page CreatePublishedPage(string title, string slug)
    {
        var page = new Page(title, slug);
        page.SetId(Guid.NewGuid());
        page.Publish();
        return page;
    }

    private static NavigationMenu CreateMenu(IEnumerable<NavigationMenuItem> items)
    {
        var menu = new NavigationMenu("Menu principal", MenuLocation.Primary);
        menu.SetId(Guid.NewGuid());

        foreach (var item in items)
            menu.MenuItems.Add(item);

        return menu;
    }

    private static NavigationMenuItem CreateMenuItem(
        string label,
        int sortOrder,
        Guid? id = null,
        Guid? parentId = null,
        Guid? pageId = null,
        string? url = null)
    {
        var itemId = id ?? Guid.NewGuid();
        var item = new NavigationMenuItem(Guid.NewGuid(), label, sortOrder);
        item.SetId(itemId);
        item.SetParentId(parentId);
        item.SetPageId(pageId);
        item.SetUrl(url);
        return item;
    }
}
