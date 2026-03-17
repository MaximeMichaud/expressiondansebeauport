using Domain.Entities;

namespace Domain.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> FindOrCreate(Guid memberAId, Guid memberBId);
    Task<Conversation?> FindById(Guid id, bool asNoTracking = true);
    Task<List<Conversation>> GetForMember(Guid memberId, int skip, int take);
    Task<int> GetUnreadCount(Guid memberId);
    Task Add(Conversation conversation);
}
