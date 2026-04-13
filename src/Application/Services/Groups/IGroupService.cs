using Domain.Entities;

namespace Application.Services.Groups;

public interface IGroupService
{
    Task<Group> CreateGroup(string name, string? description, string season, string? inviteCode, string? imageUrl);
    Task UpdateGroup(Guid groupId, string name, string? description, string season, string? imageUrl);
    Task DeleteGroup(Guid groupId);
    Task SetImageForMember(Guid groupId, Guid memberId, string imageUrl);
    Task ClearImageForMember(Guid groupId, Guid memberId);
    Task AssignProfessor(Guid groupId, Guid memberId);
    Task RemoveProfessor(Guid groupId, Guid memberId);
    Task<Group> JoinByInviteCode(string code, Guid memberId);
    Task LeaveGroup(Guid groupId, Guid memberId);
    Task ArchiveSeason(string season);
    Task<List<Group>> GetGroupsForMember(Guid memberId);
    Task<List<Group>> GetActiveGroups();
    Task<List<Group>> GetAllGroups();
    Task<Group?> GetGroupById(Guid groupId);
    Task<List<string>> GetDistinctSeasons();
}
