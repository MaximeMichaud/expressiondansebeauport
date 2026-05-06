using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Persistence;

namespace Infrastructure.Repositories.AuditLogs;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly GarneauTemplateDbContext _context;

    public AuditLogRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLog>> GetPaginated(
        int pageIndex,
        int pageSize,
        string? userQuery = null,
        string? actionType = null,
        Instant? fromInclusive = null,
        Instant? toExclusive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userQuery))
        {
            var pattern = $"%{userQuery.Trim()}%";
            query = query.Where(x =>
                (x.UserDisplayName != null && EF.Functions.ILike(x.UserDisplayName, pattern)) ||
                (x.UserEmail != null && EF.Functions.ILike(x.UserEmail, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            var normalizedActionType = actionType.Trim();
            query = query.Where(x => x.ActionType == normalizedActionType);
        }

        if (fromInclusive.HasValue)
            query = query.Where(x => x.CreatedAt >= fromInclusive.Value);

        if (toExclusive.HasValue)
            query = query.Where(x => x.CreatedAt < toExclusive.Value);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<AuditLog>(items, totalItems);
    }

    public async Task Create(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteOlderThan(Instant cutoff)
    {
        return await _context.AuditLogs
            .Where(x => x.CreatedAt < cutoff)
            .ExecuteDeleteAsync();
    }
}
