using Domain.Common;
using Domain.Entities.Identity;
using Domain.Helpers;
using NodaTime;

namespace Domain.Entities.Authentication;

public class RefreshToken : AuditableEntity
{
    public string Token { get; private set; } = default!;
    public Instant ExpiresAt { get; private set; }

    public Guid UserId { get; private set; } = default!;
    public User User { get; private set; } = default!;

    private RefreshToken()
    {
    }

    public RefreshToken(User user, string token, Instant? expiresAt = null)
    {
        User = user;
        Token = token;
        ExpiresAt = expiresAt ?? InstantHelper.GetLocalNow().Plus(Duration.FromDays(2));
    }
}