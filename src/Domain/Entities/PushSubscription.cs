using Domain.Common;
using Domain.Entities.Identity;
using NodaTime;

namespace Domain.Entities;

public class PushSubscription : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Endpoint { get; private set; } = null!;
    public string P256dh { get; private set; } = null!;
    public string Auth { get; private set; } = null!;
    public Instant LastUsedAt { get; private set; }

    private PushSubscription() { }

    public PushSubscription(Guid userId, string endpoint, string p256dh, string auth)
    {
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        LastUsedAt = Domain.Helpers.InstantHelper.GetLocalNow();
    }

    public void TouchLastUsed() => LastUsedAt = Domain.Helpers.InstantHelper.GetLocalNow();

    public void UpdateKeys(string p256dh, string auth)
    {
        P256dh = p256dh;
        Auth = auth;
    }
}
