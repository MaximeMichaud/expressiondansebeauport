using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Pages;

public class PageRepository : IPageRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PageRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public PaginatedList<Page> GetAllPaginated(int pageIndex, int pageSize, PageStatus? status = null)
    {
        var query = _context.Pages
            .Include(p => p.FeaturedImage)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var items = query.OrderByDescending(p => p.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<Page>(items, query.Count());
    }

    public List<Page> GetPublished()
    {
        return _context.Pages
            .Include(p => p.FeaturedImage)
            .AsNoTracking()
            .Where(p => p.Status == PageStatus.Published)
            .OrderBy(p => p.SortOrder)
            .ToList();
    }

    public Page? FindById(Guid id)
    {
        return _context.Pages
            .Include(p => p.FeaturedImage)
            .FirstOrDefault(p => p.Id == id);
    }

    public Page? FindBySlug(string slug)
    {
        return _context.Pages
            .Include(p => p.FeaturedImage)
            .AsNoTracking()
            .FirstOrDefault(p => p.Slug == slug);
    }

    public bool SlugExists(string slug, Guid? excludeId = null)
    {
        var query = _context.Pages.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return query.Any();
    }

    public async Task Create(Page page)
    {
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Page page)
    {
        _context.Pages.Update(page);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Page page)
    {
        _context.Pages.Remove(page);
        await _context.SaveChangesAsync();
    }
}
