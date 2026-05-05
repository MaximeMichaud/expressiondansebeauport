using Domain.Common;
using Domain.Entities.Identity;

namespace Domain.Entities;

public class UserNotificationPreferences : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public bool NotifyOnDirectMessage { get; private set; } = true;
    public bool NotifyOnAnnouncement { get; private set; } = true;
    public bool NotifyOnGroupPost { get; private set; } = true;

    private UserNotificationPreferences() { }

    public UserNotificationPreferences(Guid userId)
    {
        UserId = userId;
    }

    public void UpdatePreferences(bool dm, bool announcement, bool groupPost)
    {
        NotifyOnDirectMessage = dm;
        NotifyOnAnnouncement = announcement;
        NotifyOnGroupPost = groupPost;
    }
}
