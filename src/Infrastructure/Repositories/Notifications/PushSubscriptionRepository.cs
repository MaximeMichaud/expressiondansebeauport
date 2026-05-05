using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Notifications;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PushSubscriptionRepository(GarneauTemplateDbContext context) => _context = context;

    public Task<PushSubscription?> FindByEndpoint(string endpoint)
        => _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);

    public Task<List<PushSubscription>> GetByUserId(Guid userId)
        => _context.PushSubscriptions.AsNoTracking().Where(s => s.UserId == userId).ToListAsync();

    public async Task Add(PushSubscription subscription)
    {
        _context.PushSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task Update(PushSubscription subscription)
    {
        _context.PushSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByEndpoint(string endpoint)
    {
        var sub = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (sub != null)
        {
            _context.PushSubscriptions.Remove(sub);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteById(Guid id)
    {
        var sub = await _context.PushSubscriptions.FindAsync(id);
        if (sub != null)
        {
            _context.PushSubscriptions.Remove(sub);
            await _context.SaveChangesAsync();
        }
    }
}
