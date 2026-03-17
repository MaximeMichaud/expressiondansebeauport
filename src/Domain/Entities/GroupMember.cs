using Domain.Common;
using Domain.Enums;
using NodaTime;

namespace Domain.Entities;

public class GroupMember : Entity
{
    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public GroupMemberRole Role { get; private set; }
    public Instant JoinedAt { get; private set; }

    public void SetGroup(Group group)
    {
        Group = group;
        GroupId = group.Id;
    }

    public void SetMember(Member member)
    {
        Member = member;
        MemberId = member.Id;
    }

    public void SetRole(GroupMemberRole role) => Role = role;
    public void SetJoinedAt(Instant joinedAt) => JoinedAt = joinedAt;
}
