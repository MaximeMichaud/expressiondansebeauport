using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public class Conversation : Entity
{
    public Instant CreatedAt { get; private set; }
    public Guid ParticipantAMemberId { get; private set; }
    public Member ParticipantA { get; private set; } = null!;
    public Guid ParticipantBMemberId { get; private set; }
    public Member ParticipantB { get; private set; } = null!;

    public ICollection<ConversationParticipant> Participants { get; private set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;

    public void SetParticipants(Member a, Member b)
    {
        if (a.Id.CompareTo(b.Id) < 0)
        {
            ParticipantA = a;
            ParticipantAMemberId = a.Id;
            ParticipantB = b;
            ParticipantBMemberId = b.Id;
        }
        else
        {
            ParticipantA = b;
            ParticipantAMemberId = b.Id;
            ParticipantB = a;
            ParticipantBMemberId = a.Id;
        }
    }
}
