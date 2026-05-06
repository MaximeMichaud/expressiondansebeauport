using Domain.Common;
using Domain.Entities;
using NodaTime;

namespace Domain.Repositories;

public interface IAuditLogRepository
{
    PaginatedList<AuditLog> GetPaginated(
        int pageIndex,
        int pageSize,
        string? userQuery = null,
        string? actionType = null,
        Instant? fromInclusive = null,
        Instant? toExclusive = null);

    Task Create(AuditLog auditLog);
    Task DeleteOlderThan(Instant cutoff);
}
