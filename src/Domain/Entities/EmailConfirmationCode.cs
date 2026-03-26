using Domain.Common;
using Domain.Entities.Identity;
using NodaTime;

namespace Domain.Entities;

public class EmailConfirmationCode : Entity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public Instant ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public int ResendCount { get; private set; }
    public Instant? LastResentAt { get; private set; }
    public int AttemptCount { get; private set; }

    public void SetUser(User user)
    {
        User = user;
        UserId = user.Id;
    }

    public void SetCode(string code) => Code = code;
    public void SetExpiresAt(Instant expiresAt) => ExpiresAt = expiresAt;
    public void MarkAsUsed() => IsUsed = true;

    public void IncrementResendCount(Instant now)
    {
        ResendCount++;
        LastResentAt = now;
    }

    public void IncrementAttemptCount() => AttemptCount++;
    public bool IsExpired(Instant now) => now >= ExpiresAt;
    public bool IsLockedOut() => AttemptCount >= 5;
    public bool CanResend() => ResendCount < 3;
}
