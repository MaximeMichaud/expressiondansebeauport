using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Domain.Repositories;

namespace Application.Services.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IMemberRepository _memberRepository;

    public GroupService(
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        IMemberRepository memberRepository)
    {
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Group> CreateGroup(string name, string? description, string season, string? inviteCode, string? imageUrl)
    {
        var code = inviteCode ?? GenerateInviteCode();

        var existing = await _groupRepository.FindByInviteCode(code);
        if (existing != null)
            throw new InvalidOperationException($"Invite code '{code}' already exists.");

        var group = new Group();
        group.SetName(name);
        group.SetDescription(description);
        group.SetSeason(season);
        group.SetInviteCode(code);
        group.SetImageUrl(imageUrl);

        await _groupRepository.Add(group);
        return group;
    }

    public async Task UpdateGroup(Guid groupId, string name, string? description, string season, string? imageUrl)
    {
        var group = await _groupRepository.FindById(groupId, asNoTracking: false);
        if (group == null) throw new InvalidOperationException("Group not found.");

        group.SetName(name);
        group.SetDescription(description);
        group.SetSeason(season);
        group.SetImageUrl(imageUrl);

        await _groupRepository.Update(group);
    }

    public async Task AssignProfessor(Guid groupId, Guid memberId)
    {
        var member = _memberRepository.FindById(memberId);
        if (member == null) throw new InvalidOperationException("Member not found.");

        if (!member.User.HasRole(Domain.Constants.User.Roles.PROFESSOR))
            throw new InvalidOperationException("Member is not a professor.");

        var existing = await _groupMemberRepository.FindByGroupAndMember(groupId, memberId);
        if (existing != null) return;

        var group = await _groupRepository.FindById(groupId, asNoTracking: false);
        if (group == null) throw new InvalidOperationException("Group not found.");

        var gm = new GroupMember();
        gm.SetGroup(group);
        gm.SetMember(member);
        gm.SetRole(GroupMemberRole.Professor);
        gm.SetJoinedAt(InstantHelper.GetLocalNow());

        await _groupMemberRepository.Add(gm);
    }

    public async Task RemoveProfessor(Guid groupId, Guid memberId)
    {
        var gm = await _groupMemberRepository.FindByGroupAndMember(groupId, memberId);
        if (gm == null) return;
        await _groupMemberRepository.Remove(gm);
    }

    public async Task<Group> JoinByInviteCode(string code, Guid memberId)
    {
        var group = await _groupRepository.FindByInviteCode(code);
        if (group == null)
            throw new InvalidOperationException("Invalid invite code.");

        var alreadyMember = await _groupMemberRepository.IsMember(group.Id, memberId);
        if (alreadyMember)
            throw new InvalidOperationException("Already a member of this group.");

        var member = _memberRepository.FindById(memberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var gm = new GroupMember();
        gm.SetId(Guid.NewGuid());
        gm.SetGroupId(group.Id);
        gm.SetMemberId(member.Id);
        gm.SetRole(GroupMemberRole.Member);
        gm.SetJoinedAt(InstantHelper.GetLocalNow());

        await _groupMemberRepository.Add(gm);
        return group;
    }

    public async Task ArchiveSeason(string season)
    {
        var groups = await _groupRepository.GetBySeason(season);
        foreach (var group in groups)
        {
            group.Archive();
            await _groupRepository.Update(group);
        }
    }

    public async Task<List<Group>> GetGroupsForMember(Guid memberId)
    {
        return await _groupMemberRepository.GetGroupsForMember(memberId);
    }

    public async Task<List<Group>> GetActiveGroups()
    {
        return await _groupRepository.GetActive();
    }

    public async Task<List<Group>> GetAllGroups()
    {
        return await _groupRepository.GetAll();
    }

    public async Task<Group?> GetGroupById(Guid groupId)
    {
        return await _groupRepository.FindById(groupId);
    }

    public async Task<List<string>> GetDistinctSeasons()
    {
        return await _groupRepository.GetDistinctSeasons();
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[8];
        for (var i = 0; i < code.Length; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];
        return new string(code);
    }
}
