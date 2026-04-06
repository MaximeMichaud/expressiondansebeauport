using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public class PreviewToken : Entity
{
    public Guid PageId { get; private set; }
    public Page Page { get; private set; } = null!;

    public string Token { get; private set; } = null!;
    public Instant ExpiresAt { get; private set; }
    public Instant CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public PreviewToken() { }

    public PreviewToken(Guid pageId, Duration lifetime, string? createdBy, Instant now)
    {
        PageId = pageId;
        Token = Guid.NewGuid().ToString("N");
        CreatedAt = now;
        ExpiresAt = now + lifetime;
        CreatedBy = createdBy;
    }

    public bool IsExpired(Instant now) => now >= ExpiresAt;
}
