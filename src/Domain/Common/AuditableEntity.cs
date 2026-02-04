using NodaTime;

namespace Domain.Common;

public abstract class AuditableEntity : Entity
{
    public Instant Created { get; set; }
    public string? CreatedBy { get; set; }
    public Instant? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}