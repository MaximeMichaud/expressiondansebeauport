using Domain.Entities;

namespace Domain.Repositories;

public interface INavigationMenuRepository
{
    List<NavigationMenu> GetAll();
    NavigationMenu? FindById(Guid id, bool includeItems = false);
    NavigationMenu? FindByLocation(MenuLocation location, bool includeItems = true);
    Task Create(NavigationMenu menu);
    Task Update(NavigationMenu menu);
    Task Delete(NavigationMenu menu);
}
