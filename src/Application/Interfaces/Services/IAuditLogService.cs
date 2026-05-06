namespace Application.Interfaces.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string actionType,
        string entityType,
        Guid? entityId = null,
        string? details = null,
        Guid? userId = null,
        string? userDisplayName = null,
        string? userEmail = null);

    Task PurgeExpiredLogsAsync();
}
