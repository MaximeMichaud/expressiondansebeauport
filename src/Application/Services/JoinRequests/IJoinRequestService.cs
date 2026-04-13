using Domain.Entities;

namespace Application.Services.JoinRequests;

public interface IJoinRequestService
{
    Task<JoinRequest> CreateRequest(Guid groupId, Guid requesterMemberId);
    Task AcceptRequest(Guid joinRequestId, Guid professorMemberId);
    Task RejectRequest(Guid joinRequestId, Guid professorMemberId);
    Task<JoinRequest?> GetPendingRequest(Guid groupId, Guid memberId);
    Task<JoinRequest?> GetJoinRequestById(Guid id);
    Task<List<GroupMember>> GetProfessorsForGroup(Guid groupId);
    Task<List<Guid>> GetRecipientMemberIds(Guid groupId);
}
