using Domain.Common;

namespace Domain.Entities;

public class Message : AuditableAndSoftDeletableEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    public Guid SenderMemberId { get; private set; }
    public Member SenderMember { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string? MediaUrl { get; private set; }
    public string? MediaThumbnailUrl { get; private set; }
    public string? MediaOriginalUrl { get; private set; }

    public void SetConversation(Conversation conversation)
    {
        Conversation = conversation;
        ConversationId = conversation.Id;
    }

    public void SetSender(Member member)
    {
        SenderMember = member;
        SenderMemberId = member.Id;
    }

    public void SetContent(string content) => Content = content;
    public void SetMediaUrl(string? url) => MediaUrl = url;
    public void SetMediaThumbnailUrl(string? url) => MediaThumbnailUrl = url;
    public void SetMediaOriginalUrl(string? url) => MediaOriginalUrl = url;
}
