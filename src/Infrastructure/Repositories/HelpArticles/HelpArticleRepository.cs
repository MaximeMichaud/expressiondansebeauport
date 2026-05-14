using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.HelpArticles;

public class HelpArticleRepository : IHelpArticleRepository
{
    private readonly GarneauTemplateDbContext _context;

    public HelpArticleRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task<HelpArticle?> GetById(Guid id)
    {
        return await _context.HelpArticles.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<HelpArticle?> GetBySlug(string slug)
    {
        return await _context.HelpArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Slug == slug);
    }

    public async Task<HelpArticle?> GetByRouteHint(string routeName)
    {
        return await _context.HelpArticles
            .AsNoTracking()
            .Where(a => a.RouteHint == routeName && a.IsPublished)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SlugExists(string slug, Guid? excludeId = null)
    {
        return await _context.HelpArticles
            .AsNoTracking()
            .AnyAsync(a => a.Slug == slug && (excludeId == null || a.Id != excludeId.Value));
    }

    public async Task<List<HelpArticle>> GetAll(HelpCategory? category = null, bool? isPublished = null)
    {
        var query = _context.HelpArticles.AsNoTracking().AsQueryable();

        if (category.HasValue)
            query = query.Where(a => a.Category == category.Value);

        if (isPublished.HasValue)
            query = query.Where(a => a.IsPublished == isPublished.Value);

        return await query
            .OrderBy(a => a.Category)
            .ThenBy(a => a.SortOrder)
            .ThenBy(a => a.Title)
            .ToListAsync();
    }

    public async Task Create(HelpArticle article)
    {
        _context.HelpArticles.Add(article);
        await _context.SaveChangesAsync();
    }

    public async Task Update(HelpArticle article)
    {
        _context.HelpArticles.Update(article);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var article = await _context.HelpArticles.FirstOrDefaultAsync(a => a.Id == id);
        if (article is null) return;

        _context.HelpArticles.Remove(article);
        await _context.SaveChangesAsync();
    }
}
