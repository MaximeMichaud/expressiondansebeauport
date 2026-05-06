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

    public PaginatedList<AuditLog> GetPaginated(
        int pageIndex,
        int pageSize,
        string? userQuery = null,
        string? actionType = null,
        Instant? fromInclusive = null,
        Instant? toExclusive = null)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userQuery))
        {
            var normalized = userQuery.Trim().ToLower();
            query = query.Where(x =>
                (x.UserDisplayName != null && x.UserDisplayName.ToLower().Contains(normalized)) ||
                (x.UserEmail != null && x.UserEmail.ToLower().Contains(normalized)));
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            var normalizedActionType = actionType.Trim().ToLower();
            query = query.Where(x => x.ActionType.ToLower() == normalizedActionType);
        }

        if (fromInclusive.HasValue)
            query = query.Where(x => x.CreatedAt >= fromInclusive.Value);

        if (toExclusive.HasValue)
            query = query.Where(x => x.CreatedAt < toExclusive.Value);

        var totalItems = query.Count();
        var items = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<AuditLog>(items, totalItems);
    }

    public async Task Create(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOlderThan(Instant cutoff)
    {
        var expiredLogs = await _context.AuditLogs
            .Where(x => x.CreatedAt < cutoff)
            .ToListAsync();

        if (expiredLogs.Count == 0)
            return;

        _context.AuditLogs.RemoveRange(expiredLogs);
        await _context.SaveChangesAsync();
    }
}
