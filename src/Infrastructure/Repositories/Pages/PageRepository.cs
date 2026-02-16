using Application.Exceptions.Pages;
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

    public List<Page> GetAll()
    {
        return _context.Pages
            .Include(p => p.Sections.OrderBy(s => s.SortOrder))
            .AsNoTracking()
            .OrderBy(p => p.Title)
            .ToList();
    }

    public Page FindById(Guid id)
    {
        var page = _context.Pages
            .Include(p => p.Sections.OrderBy(s => s.SortOrder))
            .FirstOrDefault(p => p.Id == id);
        if (page == null)
            throw new PageNotFoundException($"No page with id {id} was found.");
        return page;
    }

    public Page FindBySlug(string slug)
    {
        var page = _context.Pages
            .Include(p => p.Sections.OrderBy(s => s.SortOrder))
            .AsNoTracking()
            .FirstOrDefault(p => p.Slug == slug);
        if (page == null)
            throw new PageNotFoundException($"No page with slug '{slug}' was found.");
        return page;
    }

    public async Task Update(Page page)
    {
        _context.Pages.Update(page);
        await _context.SaveChangesAsync();
    }
}
