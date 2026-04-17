using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence;

namespace Infrastructure.Repositories.Pages;

public class PageRevisionRepository : IPageRevisionRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PageRevisionRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public List<PageRevision> GetByPageId(Guid pageId)
    {
        return _context.PageRevisions
            .AsNoTracking()
            .Where(r => r.PageId == pageId && r.RevisionType == RevisionType.Manual)
            .OrderByDescending(r => r.RevisionNumber)
            .ToList();
    }

    public PageRevision? FindById(Guid id)
    {
        return _context.PageRevisions.FirstOrDefault(r => r.Id == id);
    }

    public PageRevision? GetLatestByPageId(Guid pageId, RevisionType? type = null)
    {
        var query = _context.PageRevisions
            .AsNoTracking()
            .Where(r => r.PageId == pageId);

        if (type.HasValue)
            query = query.Where(r => r.RevisionType == type.Value);

        return query.OrderByDescending(r => r.RevisionNumber).FirstOrDefault();
    }

    public int GetNextRevisionNumber(Guid pageId)
    {
        var max = _context.PageRevisions
            .Where(r => r.PageId == pageId && r.RevisionType == RevisionType.Manual)
            .Max(r => (int?)r.RevisionNumber) ?? 0;
        return max + 1;
    }

    public async Task Create(PageRevision revision)
    {
        _context.PageRevisions.Add(revision);
        await _context.SaveChangesAsync();
    }

    public async Task UpsertAutosave(PageRevision revision, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var existing = await _context.PageRevisions
            .FirstOrDefaultAsync(r => r.PageId == revision.PageId && r.RevisionType == RevisionType.Autosave, ct);

        if (existing is not null)
            _context.PageRevisions.Remove(existing);

        _context.PageRevisions.Add(revision);

        try
        {
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // Race condition : une autre requête a inséré entre notre DELETE et notre INSERT
            await tx.RollbackAsync(ct);
            _context.Entry(revision).State = EntityState.Detached;

            // Ne remplacer que si notre révision est plus récente que la survivante
            var surviving = await _context.PageRevisions
                .Where(r => r.PageId == revision.PageId && r.RevisionType == RevisionType.Autosave)
                .FirstOrDefaultAsync(ct);

            if (surviving is null || revision.CreatedAt >= surviving.CreatedAt)
            {
                await _context.PageRevisions
                    .Where(r => r.PageId == revision.PageId && r.RevisionType == RevisionType.Autosave)
                    .ExecuteDeleteAsync(ct);
                _context.PageRevisions.Add(revision);
                await _context.SaveChangesAsync(ct);
            }
        }
    }

    public async Task DeleteOldRevisions(Guid pageId, int keepCount)
    {
        var toDelete = _context.PageRevisions
            .Where(r => r.PageId == pageId && r.RevisionType == RevisionType.Manual)
            .OrderByDescending(r => r.RevisionNumber)
            .Skip(keepCount)
            .ToList();

        if (toDelete.Count > 0)
        {
            _context.PageRevisions.RemoveRange(toDelete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAutosave(Guid pageId)
    {
        var autosaves = _context.PageRevisions
            .Where(r => r.PageId == pageId && r.RevisionType == RevisionType.Autosave)
            .ToList();

        if (autosaves.Count > 0)
        {
            _context.PageRevisions.RemoveRange(autosaves);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PageRevision?> GetAutosave(Guid pageId)
    {
        return await _context.PageRevisions
            .AsNoTracking()
            .Where(r => r.PageId == pageId && r.RevisionType == RevisionType.Autosave)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
