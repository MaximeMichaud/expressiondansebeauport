using Domain.Entities;
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

    public async Task<Message> SendMessage(Guid conversationId, Guid senderMemberId, string content)
    {
        var conversation = await _conversationRepository.FindById(conversationId, asNoTracking: false);
        if (conversation == null) throw new InvalidOperationException("Conversation not found.");

        var sender = _memberRepository.FindById(senderMemberId, asNoTracking: false);
        if (sender == null) throw new InvalidOperationException("Sender not found.");

        var isParticipant = conversation.ParticipantAMemberId == senderMemberId ||
                           conversation.ParticipantBMemberId == senderMemberId;
        if (!isParticipant) throw new InvalidOperationException("Not a participant in this conversation.");

        var message = new Message();
        message.SetConversation(conversation);
        message.SetSender(sender);
        message.SetContent(content);

        await _messageRepository.Add(message);

        // Mark as read for sender
        await _messageRepository.MarkAsRead(conversationId, senderMemberId);

        return message;
    }

    public async Task<List<Conversation>> GetConversations(Guid memberId, int page)
    {
        var skip = (page - 1) * 20;
        return await _conversationRepository.GetForMember(memberId, skip, 20);
    }

    public async Task<List<Message>> GetMessages(Guid conversationId, int page)
    {
        var skip = (page - 1) * 30;
        return await _messageRepository.GetByConversation(conversationId, skip, 30);
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
