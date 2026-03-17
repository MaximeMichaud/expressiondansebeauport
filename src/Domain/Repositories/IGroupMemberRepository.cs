using Domain.Entities;

namespace Domain.Repositories;

public interface IGroupMemberRepository
{
    Task Add(GroupMember groupMember);
    Task<bool> IsMember(Guid groupId, Guid memberId);
    Task<List<Group>> GetGroupsForMember(Guid memberId);
    Task<List<GroupMember>> GetMembersOfGroup(Guid groupId, int skip = 0, int take = 50);
    Task<int> GetMemberCount(Guid groupId);
    Task<GroupMember?> FindByGroupAndMember(Guid groupId, Guid memberId);
    Task Remove(GroupMember groupMember);
    Task<GroupMember?> FindProfessorInGroup(Guid groupId, Guid memberId);
}
