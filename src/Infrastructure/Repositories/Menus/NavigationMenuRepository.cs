using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Menus;

public class NavigationMenuRepository : INavigationMenuRepository
{
    private readonly GarneauTemplateDbContext _context;

    public NavigationMenuRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public List<NavigationMenu> GetAll()
    {
        return _context.NavigationMenus.AsNoTracking().ToList();
    }

    public NavigationMenu? FindById(Guid id, bool includeItems = false)
    {
        var query = _context.NavigationMenus.AsQueryable();
        if (includeItems)
            query = query.Include(m => m.MenuItems.OrderBy(i => i.SortOrder))
                         .ThenInclude(i => i.Page);
        return query.FirstOrDefault(m => m.Id == id);
    }

    public NavigationMenu? FindByLocation(MenuLocation location, bool includeItems = true)
    {
        var query = _context.NavigationMenus.AsQueryable();
        if (includeItems)
            query = query.Include(m => m.MenuItems.OrderBy(i => i.SortOrder))
                         .ThenInclude(i => i.Page);
        return query.FirstOrDefault(m => m.Location == location);
    }

    public async Task Create(NavigationMenu menu)
    {
        _context.NavigationMenus.Add(menu);
        await _context.SaveChangesAsync();
    }

    public async Task Update(NavigationMenu menu)
    {
        _context.NavigationMenus.Update(menu);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(NavigationMenu menu)
    {
        _context.NavigationMenus.Remove(menu);
        await _context.SaveChangesAsync();
    }
}
