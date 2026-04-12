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
            throw new InvalidOperationException("Groupe non trouvé.");

        var isMember = await _groupMemberRepository.IsMember(groupId, requesterMemberId);
        if (isMember)
            throw new InvalidOperationException("Déjà membre de ce groupe.");

        var existing = await _joinRequestRepository.FindPendingByGroupAndMember(groupId, requesterMemberId);
        if (existing != null)
            throw new InvalidOperationException("DUPLICATE");

        var requester = _memberRepository.FindById(requesterMemberId);
        if (requester == null)
            throw new InvalidOperationException("Membre non trouvé.");

        // Collect recipient member IDs — professors/admins in group first, fallback to global admins
        var groupStaff = await _groupMemberRepository.GetProfessorsAndAdminsInGroup(groupId);
        var recipientMemberIds = groupStaff.Select(p => p.MemberId).ToList();

        if (recipientMemberIds.Count == 0)
        {
            var admins = await _memberRepository.GetAdminMembers();
            recipientMemberIds = admins.Select(a => a.Id).ToList();
        }

        if (recipientMemberIds.Count == 0)
            throw new InvalidOperationException("Aucun professeur ou administrateur disponible.");

        var joinRequest = new JoinRequest();
        joinRequest.SetId(Guid.NewGuid());
        joinRequest.SetGroupId(groupId);
        joinRequest.SetRequesterMemberId(requesterMemberId);
        joinRequest.SetStatus(JoinRequestStatus.Pending);
        joinRequest.SetCreatedAt(InstantHelper.GetLocalNow());

        await _joinRequestRepository.Add(joinRequest);

        var content = $"{requester.FullName} souhaite rejoindre le groupe {group.Name}";

        foreach (var recipientId in recipientMemberIds)
        {
            var conversation = await _conversationService.GetOrCreateConversation(requesterMemberId, recipientId);
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
            throw new InvalidOperationException("Demande d'adhésion non trouvée.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Demande d'adhésion déjà traitée.");

        // Allow professors/admins in the group (by platform role) OR global admins
        var actingMember = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (actingMember == null)
            throw new InvalidOperationException("Membre non trouvé.");

        var isGroupStaff = await _groupMemberRepository.IsMember(joinRequest.GroupId, professorMemberId)
            && (actingMember.User.HasRole(Domain.Constants.User.Roles.PROFESSOR)
                || actingMember.User.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR));
        var isGlobalAdmin = actingMember.User.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR);

        if (!isGroupStaff && !isGlobalAdmin)
            throw new InvalidOperationException("Non autorisé à accepter cette demande.");

        var professor = actingMember;
        if (professor == null)
            throw new InvalidOperationException("Professeur non trouvé.");

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
                $"{professor.FullName} a accepté votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>(),
                MessageType.JoinRequest,
                joinRequest.Id);
        }
    }

    public async Task RejectRequest(Guid joinRequestId, Guid professorMemberId)
    {
        var joinRequest = await _joinRequestRepository.FindById(joinRequestId, asNoTracking: false);
        if (joinRequest == null)
            throw new InvalidOperationException("Demande d'adhésion non trouvée.");

        if (joinRequest.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Demande d'adhésion déjà traitée.");

        // Allow professors/admins in the group (by platform role) OR global admins
        var actingMember = _memberRepository.FindById(professorMemberId, asNoTracking: false);
        if (actingMember == null)
            throw new InvalidOperationException("Membre non trouvé.");

        var isGroupStaff = await _groupMemberRepository.IsMember(joinRequest.GroupId, professorMemberId)
            && (actingMember.User.HasRole(Domain.Constants.User.Roles.PROFESSOR)
                || actingMember.User.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR));
        var isGlobalAdmin = actingMember.User.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR);

        if (!isGroupStaff && !isGlobalAdmin)
            throw new InvalidOperationException("Non autorisé à rejeter cette demande.");

        var professor = actingMember;

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
                $"{professor.FullName} a refusé votre demande pour {group?.Name ?? "le groupe"}",
                new List<MessageMediaItem>(),
                MessageType.JoinRequest,
                joinRequest.Id);
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
        var professors = await _groupMemberRepository.GetProfessorsOfGroup(groupId);
        return professors;
    }

    public async Task<List<Guid>> GetRecipientMemberIds(Guid groupId)
    {
        var groupStaff = await _groupMemberRepository.GetProfessorsAndAdminsInGroup(groupId);
        if (groupStaff.Count > 0)
            return groupStaff.Select(p => p.MemberId).ToList();

        var admins = await _memberRepository.GetAdminMembers();
        return admins.Select(a => a.Id).ToList();
    }
}
