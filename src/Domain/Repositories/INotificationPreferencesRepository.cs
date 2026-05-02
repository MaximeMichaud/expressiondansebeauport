using Domain.Entities;

namespace Domain.Repositories;

public interface INotificationPreferencesRepository
{
    Task<UserNotificationPreferences?> FindByUserId(Guid userId);
    Task<UserNotificationPreferences> GetOrCreate(Guid userId);
    Task UpdatePreferences(Guid userId, bool dm, bool announcement, bool groupPost);

    Task<List<UserGroupNotificationPreferences>> GetGroupOverridesForUser(Guid userId);
    Task<UserGroupNotificationPreferences?> FindGroupOverride(Guid userId, Guid groupId);
    Task SetGroupOverride(Guid userId, Guid groupId, bool enabled);
}
