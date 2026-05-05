using Domain.Common;
using Domain.Entities.Identity;

namespace Domain.Entities;

public class UserGroupNotificationPreferences : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;
    public bool Enabled { get; private set; }

    private UserGroupNotificationPreferences() { }

    public UserGroupNotificationPreferences(Guid userId, Guid groupId, bool enabled)
    {
        UserId = userId;
        GroupId = groupId;
        Enabled = enabled;
    }

    public void SetEnabled(bool enabled) => Enabled = enabled;
}
