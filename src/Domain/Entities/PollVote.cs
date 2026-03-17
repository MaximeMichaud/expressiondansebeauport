using Domain.Common;
using NodaTime;

namespace Domain.Entities;

public class PollVote : Entity
{
    public Guid PollOptionId { get; private set; }
    public PollOption PollOption { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public Instant CreatedAt { get; private set; }

    public void SetPollOption(PollOption option)
    {
        PollOption = option;
        PollOptionId = option.Id;
    }

    public void SetMember(Member member)
    {
        Member = member;
        MemberId = member.Id;
    }

    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;
}
