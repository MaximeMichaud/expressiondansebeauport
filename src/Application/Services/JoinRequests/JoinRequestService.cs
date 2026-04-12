using Application.Services.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Domain.Repositories;

namespace Application.Services.JoinRequests;

public class JoinRequestService : IJoinRequestService
{
    private readonly IJoinRequestRepository _joinRequestRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IConversationService _conversationService;

    public JoinRequestService(
        IJoinRequestRepository joinRequestRepository,
        IGroupMemberRepository groupMemberRepository,
        IGroupRepository groupRepository,
        IMemberRepository memberRepository,
        IConversationService conversationService)
    {
        _joinRequestRepository = joinRequestRepository;
        _groupMemberRepository = groupMemberRepository;
        _groupRepository = groupRepository;
        _memberRepository = memberRepository;
        _conversationService = conversationService;
    }

    public async Task<JoinRequest> CreateRequest(Guid groupId, Guid requesterMemberId)
    {
        var group = await _groupRepository.FindById(groupId);
        if (group == null)
            throw new InvalidOperationException("Group not found.");

        var isMember = await _groupMemberRepository.IsMember(groupId, requesterMemberId);
        if (isMember)
            throw new InvalidOperationException("Already a member of this group.");

        var existing = await _joinRequestRepository.FindPendingByGroupAndMember(groupId, requesterMemberId);
        if (existing != null)
            throw new InvalidOperationException("DUPLICATE");

        var requester = _memberRepository.FindById(requesterMemberId);
        if (requester == null)
            throw new InvalidOperationException("Member not found.");

        var professors = await _groupMemberRepository.GetProfessorsOfGroup(groupId);
        if (professors.Count == 0)
            throw new InvalidOperationException("No professors in this group.");

        var joinRequest = new JoinRequest();
        joinRequest.SetId(Guid.NewGuid());
        joinRequest.SetGroupId(groupId);
        joinRequest.SetRequesterMemberId(requesterMemberId);
        joinRequest.SetStatus(JoinRequestStatus.Pending);
        joinRequest.SetCreatedAt(InstantHelper.GetLocalNow());

        await _joinRequestRepository.Add(joinRequest);

        var content = $"{requester.FullName} souhaite rejoindre le groupe {group.Name}";

        foreach (var prof in professors)
        {
            var conversation = await _conversationService.GetOrCreateConversation(requesterMemberId, prof.MemberId);
            if (conversation == null) continue;

            await _conversationService.SendMessage(
                conversation.Id,
                requesterMemberId,
                content,
                new List<MessageMediaItem>(),
                MessageType.JoinRequest,
                joinRequest.Id);
        }

        return joinRequest;
    }

    public async Task AcceptRequest(Guid joinRequestId, Guid professorMemberId)
    {
        var joinRequest = await _joinRequestRepository.FindById(joinRequestId, asNoTracking: false);
        if (joinRequest == null)
            throw new InvalidOperationException("Join request not found.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Join request already resolved.");

        var profGm = await _groupMemberRepository.FindProfessorInGroup(joinRequest.GroupId, professorMemberId);
        if (profGm == null)
            throw new InvalidOperationException("Not a professor in this group.");

        var professor = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (professor == null)
            throw new InvalidOperationException("Professor not found.");

        joinRequest.SetStatus(JoinRequestStatus.Accepted);
        joinRequest.SetResolvedByMember(professor);
        joinRequest.SetResolvedAt(InstantHelper.GetLocalNow());
        await _joinRequestRepository.Update(joinRequest);

        var gm = new GroupMember();
        gm.SetId(Guid.NewGuid());
        gm.SetGroupId(joinRequest.GroupId);
        gm.SetMemberId(joinRequest.RequesterMemberId);
        gm.SetRole(GroupMemberRole.Member);
        gm.SetJoinedAt(InstantHelper.GetLocalNow());
        await _groupMemberRepository.Add(gm);

        var group = await _groupRepository.FindById(joinRequest.GroupId);
        var conversation = await _conversationService.GetOrCreateConversation(joinRequest.RequesterMemberId, professorMemberId);
        if (conversation != null)
        {
            await _conversationService.SendMessage(
                conversation.Id,
                professorMemberId,
                $"✅ {professor.FullName} a accepté votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>());
        }
    }

    public async Task RejectRequest(Guid joinRequestId, Guid professorMemberId)
    {
        var joinRequest = await _joinRequestRepository.FindById(joinRequestId, asNoTracking: false);
        if (joinRequest == null)
            throw new InvalidOperationException("Join request not found.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Join request already resolved.");

        var profGm = await _groupMemberRepository.FindProfessorInGroup(joinRequest.GroupId, professorMemberId);
        if (profGm == null)
            throw new InvalidOperationException("Not a professor in this group.");

        var professor = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (professor == null)
            throw new InvalidOperationException("Professor not found.");

        joinRequest.SetStatus(JoinRequestStatus.Rejected);
        joinRequest.SetResolvedByMember(professor);
        joinRequest.SetResolvedAt(InstantHelper.GetLocalNow());
        await _joinRequestRepository.Update(joinRequest);

        var group = await _groupRepository.FindById(joinRequest.GroupId);
        var conversation = await _conversationService.GetOrCreateConversation(joinRequest.RequesterMemberId, professorMemberId);
        if (conversation != null)
        {
            await _conversationService.SendMessage(
                conversation.Id,
                professorMemberId,
                $"❌ {professor.FullName} a refusé votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>());
        }
    }

    public async Task<JoinRequest?> GetPendingRequest(Guid groupId, Guid memberId)
    {
        return await _joinRequestRepository.FindPendingByGroupAndMember(groupId, memberId);
    }

    public async Task<JoinRequest?> GetJoinRequestById(Guid id)
    {
        return await _joinRequestRepository.FindById(id);
    }

    public async Task<List<GroupMember>> GetProfessorsForGroup(Guid groupId)
    {
        return await _groupMemberRepository.GetProfessorsOfGroup(groupId);
    }
}
