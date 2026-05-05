using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Notifications;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly GarneauTemplateDbContext _context;

    public NotificationPreferencesRepository(GarneauTemplateDbContext context) => _context = context;

    public Task<UserNotificationPreferences?> FindByUserId(Guid userId)
        => _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<UserNotificationPreferences> GetOrCreate(Guid userId)
    {
        var existing = await _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null) return existing;

        var fresh = new UserNotificationPreferences(userId);
        _context.UserNotificationPreferences.Add(fresh);
        await _context.SaveChangesAsync();
        return fresh;
    }

    public async Task UpdatePreferences(Guid userId, bool dm, bool announcement, bool groupPost)
    {
        var prefs = await GetOrCreate(userId);
        prefs.UpdatePreferences(dm, announcement, groupPost);
        await _context.SaveChangesAsync();
    }

    public Task<List<UserGroupNotificationPreferences>> GetGroupOverridesForUser(Guid userId)
        => _context.UserGroupNotificationPreferences.AsNoTracking().Where(g => g.UserId == userId).ToListAsync();

    public Task<UserGroupNotificationPreferences?> FindGroupOverride(Guid userId, Guid groupId)
        => _context.UserGroupNotificationPreferences.FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

    public async Task SetGroupOverride(Guid userId, Guid groupId, bool enabled)
    {
        var existing = await _context.UserGroupNotificationPreferences
            .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

        if (existing == null)
        {
            _context.UserGroupNotificationPreferences.Add(new UserGroupNotificationPreferences(userId, groupId, enabled));
        }
        else
        {
            existing.SetEnabled(enabled);
        }
        await _context.SaveChangesAsync();
    }
}
