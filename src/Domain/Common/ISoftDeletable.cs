using NodaTime;

namespace Domain.Common;

public interface ISoftDeletable
{
    Instant? Deleted { get; }
    string? DeletedBy { get; }

    void SoftDelete(string? deletedBy = null);
    void Restore();
}