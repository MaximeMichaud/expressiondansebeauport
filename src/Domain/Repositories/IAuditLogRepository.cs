using Domain.Common;
using Domain.Entities;
using NodaTime;

namespace Domain.Repositories;

public interface IAuditLogRepository
{
    Task<PaginatedList<AuditLog>> GetPaginated(
        int pageIndex,
        int pageSize,
        string? userQuery = null,
        string? actionType = null,
        Instant? fromInclusive = null,
        Instant? toExclusive = null,
        CancellationToken cancellationToken = default);

    Task Create(AuditLog auditLog);
    Task<int> DeleteOlderThan(Instant cutoff);
}
