using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;

namespace Application.Services.Messaging;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMemberRepository _memberRepository;

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IMemberRepository memberRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Conversation?> GetOrCreateConversation(Guid memberAId, Guid memberBId)
    {
        return await _conversationRepository.FindOrCreate(memberAId, memberBId);
    }

    public async Task<Message> SendMessage(
        Guid conversationId,
        Guid senderMemberId,
        string? content,
        IReadOnlyList<MessageMediaItem> media,
        MessageType messageType = MessageType.Text,
        Guid? joinRequestId = null)
    {
        if (media.Count > 10)
            throw new InvalidOperationException("A message cannot have more than 10 media items.");

        var conversation = await _conversationRepository.FindById(conversationId, asNoTracking: false);
        if (conversation == null) throw new InvalidOperationException("Conversation not found.");

        var sender = _memberRepository.FindById(senderMemberId, asNoTracking: false);
        if (sender == null) throw new InvalidOperationException("Sender not found.");

        var isParticipant = conversation.ParticipantAMemberId == senderMemberId ||
                           conversation.ParticipantBMemberId == senderMemberId;
        if (!isParticipant) throw new InvalidOperationException("Not a participant in this conversation.");

        var hasContent = !string.IsNullOrWhiteSpace(content);
        var hasMedia = media.Count > 0;
        if (!hasContent && !hasMedia)
            throw new InvalidOperationException("Message must have content or media.");

        var message = new Message();
        message.SetConversation(conversation);
        message.SetSender(sender);
        message.SetContent(content ?? string.Empty);
        message.SetMessageType(messageType);
        message.SetJoinRequestId(joinRequestId);

        for (var i = 0; i < media.Count; i++)
        {
            var item = media[i];
            var mm = new MessageMedia();
            mm.SetMessage(message);
            mm.SetMediaUrl(item.DisplayUrl);
            mm.SetThumbnailUrl(item.ThumbnailUrl);
            mm.SetOriginalUrl(item.OriginalUrl);
            mm.SetContentType(item.ContentType);
            mm.SetSize(item.Size);
            mm.SetSortOrder(i);
            message.Media.Add(mm);
        }

        await _messageRepository.Add(message);

        // Mark as read for sender
        await _messageRepository.MarkAsRead(conversationId, senderMemberId);

        return message;
    }

    public async Task<PaginatedResult<Conversation>> GetConversations(Guid memberId, int page)
    {
        const int pageSize = 20;
        var skip = (page - 1) * pageSize;
        var items = await _conversationRepository.GetForMember(memberId, skip, pageSize + 1);
        var hasMore = items.Count > pageSize;
        return new PaginatedResult<Conversation>(items.Take(pageSize).ToList(), hasMore);
    }

    public async Task<PaginatedResult<Message>> GetMessages(Guid conversationId, int page)
    {
        const int pageSize = 30;
        var skip = (page - 1) * pageSize;
        var items = await _messageRepository.GetByConversation(conversationId, skip, pageSize + 1);
        var hasMore = items.Count > pageSize;
        return new PaginatedResult<Message>(items.Take(pageSize).ToList(), hasMore);
    }

    public async Task MarkAsRead(Guid conversationId, Guid memberId)
    {
        await _messageRepository.MarkAsRead(conversationId, memberId);
    }

    public async Task<int> GetUnreadCount(Guid memberId)
    {
        return await _conversationRepository.GetUnreadCount(memberId);
    }
}
