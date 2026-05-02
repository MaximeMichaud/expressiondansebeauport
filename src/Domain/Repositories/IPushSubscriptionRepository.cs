using Domain.Entities;

namespace Domain.Repositories;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> FindByEndpoint(string endpoint);
    Task<List<PushSubscription>> GetByUserId(Guid userId);
    Task Add(PushSubscription subscription);
    Task Update(PushSubscription subscription);
    Task DeleteByEndpoint(string endpoint);
    Task DeleteById(Guid id);
}
