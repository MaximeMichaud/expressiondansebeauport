using Domain.Entities;

namespace Domain.Repositories;

public interface IMessageRepository
{
    Task Add(Message message);
    Task<Message?> FindById(Guid id);
    Task<List<Message>> GetByConversation(Guid conversationId, int skip, int take);
    Task MarkAsRead(Guid conversationId, Guid memberId);
    Task SaveChanges();
}
