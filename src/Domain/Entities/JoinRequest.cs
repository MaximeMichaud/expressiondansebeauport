using Domain.Common;
using Domain.Enums;
using NodaTime;

namespace Domain.Entities;

public class JoinRequest : Entity
{
    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;
    public Guid RequesterMemberId { get; private set; }
    public Member RequesterMember { get; private set; } = null!;
    public JoinRequestStatus Status { get; private set; }
    public Guid? ResolvedByMemberId { get; private set; }
    public Member? ResolvedByMember { get; private set; }
    public Instant? ResolvedAt { get; private set; }
    public Instant CreatedAt { get; private set; }
    public bool RequesterNotified { get; private set; }

    public void SetRequesterNotified(bool value) => RequesterNotified = value;
    public void SetGroup(Group group) { Group = group; GroupId = group.Id; }
    public void SetGroupId(Guid groupId) => GroupId = groupId;
    public void SetRequesterMember(Member member) { RequesterMember = member; RequesterMemberId = member.Id; }
    public void SetRequesterMemberId(Guid memberId) => RequesterMemberId = memberId;
    public void SetStatus(JoinRequestStatus status) => Status = status;
    public void SetResolvedByMember(Member member) { ResolvedByMember = member; ResolvedByMemberId = member.Id; }
    public void SetResolvedAt(Instant resolvedAt) => ResolvedAt = resolvedAt;
    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;
}
