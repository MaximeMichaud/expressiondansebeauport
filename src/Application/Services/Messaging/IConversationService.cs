using Domain.Entities;

namespace Application.Services.Messaging;

public interface IConversationService
{
    Task<Conversation?> GetOrCreateConversation(Guid memberAId, Guid memberBId);
    Task<Message> SendMessage(
        Guid conversationId,
        Guid senderMemberId,
        string? content,
        string? mediaUrl,
        string? mediaThumbnailUrl,
        string? mediaOriginalUrl);
    Task<List<Conversation>> GetConversations(Guid memberId, int page);
    Task<List<Message>> GetMessages(Guid conversationId, int page);
    Task MarkAsRead(Guid conversationId, Guid memberId);
    Task<int> GetUnreadCount(Guid memberId);
}
