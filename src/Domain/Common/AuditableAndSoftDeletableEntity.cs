using Domain.Helpers;
using NodaTime;

namespace Domain.Common;

public abstract class AuditableAndSoftDeletableEntity : Entity, ISoftDeletable
{
    public Instant Created { get; set; }
    public string? CreatedBy { get; set; }
    public Instant? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public Instant? Deleted { get; set; }
    public string? DeletedBy { get; set; }
    
    public virtual void SoftDelete(string? deletedBy = null)
    {
        Deleted = InstantHelper.GetLocalNow();
        DeletedBy = deletedBy;
    }

    public virtual void Restore()
    {
        Deleted = null;
        DeletedBy = null;
    }
}