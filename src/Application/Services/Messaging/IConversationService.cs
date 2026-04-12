using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Messaging;

public class MessageMediaItem
{
    public string DisplayUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}

public interface IConversationService
{
    Task<Conversation?> GetOrCreateConversation(Guid memberAId, Guid memberBId);
    Task<Message> SendMessage(
        Guid conversationId,
        Guid senderMemberId,
        string? content,
        IReadOnlyList<MessageMediaItem> media,
        MessageType messageType = MessageType.Text,
        Guid? joinRequestId = null);
    Task<List<Conversation>> GetConversations(Guid memberId, int page);
    Task<List<Message>> GetMessages(Guid conversationId, int page);
    Task MarkAsRead(Guid conversationId, Guid memberId);
    Task<int> GetUnreadCount(Guid memberId);
}
