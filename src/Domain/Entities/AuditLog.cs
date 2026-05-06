using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public class AuditLog : Entity
{
    public Guid? UserId { get; private set; }
    public string? UserDisplayName { get; private set; }
    public string? UserEmail { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public Guid? EntityId { get; private set; }
    public string? Details { get; private set; }
    public Instant CreatedAt { get; private set; }

    public AuditLog()
    {
    }

    public AuditLog(
        Guid? userId,
        string? userDisplayName,
        string? userEmail,
        string actionType,
        string entityType,
        Guid? entityId,
        string? details,
        Instant createdAt)
    {
        UserId = userId;
        UserDisplayName = userDisplayName;
        UserEmail = userEmail;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Details = details;
        CreatedAt = createdAt;
    }
}
