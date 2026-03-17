using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public class ConversationParticipant : Entity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public Instant? LastReadAt { get; private set; }
    public Instant JoinedAt { get; private set; }

    public void SetConversation(Conversation conversation)
    {
        Conversation = conversation;
        ConversationId = conversation.Id;
    }

    public void SetMember(Member member)
    {
        Member = member;
        MemberId = member.Id;
    }

    public void SetLastReadAt(Instant? lastReadAt) => LastReadAt = lastReadAt;
    public void SetJoinedAt(Instant joinedAt) => JoinedAt = joinedAt;
}
