using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IJoinRequestRepository
{
    Task Add(JoinRequest joinRequest);
    Task<JoinRequest?> FindById(Guid id, bool asNoTracking = true);
    Task<JoinRequest?> FindPendingByGroupAndMember(Guid groupId, Guid memberId);
    Task Update(JoinRequest joinRequest);
    Task<List<JoinRequest>> FindUnnotifiedResolved(Guid requesterMemberId);
    Task MarkNotified(Guid joinRequestId);
}
