using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Groups;

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly GarneauTemplateDbContext _context;

    public GroupMemberRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(GroupMember groupMember)
    {
        _context.GroupMembers.Add(groupMember);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsMember(Guid groupId, Guid memberId)
    {
        return await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.MemberId == memberId);
    }

    public async Task<List<Group>> GetGroupsForMember(Guid memberId)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.MemberId == memberId && !gm.Group.IsArchived)
            .Include(gm => gm.Group).ThenInclude(g => g.Members)
            .Select(gm => gm.Group)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<List<GroupMember>> GetMembersOfGroup(Guid groupId, int skip = 0, int take = 50)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.GroupId == groupId)
            .Include(gm => gm.Member).ThenInclude(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(gm => gm.Member.LastName).ThenBy(gm => gm.Member.FirstName)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task<int> GetMemberCount(Guid groupId)
    {
        return await _context.GroupMembers.CountAsync(gm => gm.GroupId == groupId);
    }

    public async Task<GroupMember?> FindByGroupAndMember(Guid groupId, Guid memberId)
    {
        return await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.MemberId == memberId);
    }

    public async Task Remove(GroupMember groupMember)
    {
        _context.GroupMembers.Remove(groupMember);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<Guid, string>> GetRolesForMembers(Guid groupId, List<Guid> memberIds)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.GroupId == groupId && memberIds.Contains(gm.MemberId))
            .ToDictionaryAsync(gm => gm.MemberId, gm => gm.Role.ToString());
    }

    public async Task<GroupMember?> FindProfessorInGroup(Guid groupId, Guid memberId)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(gm =>
                gm.GroupId == groupId &&
                gm.MemberId == memberId &&
                gm.Role == GroupMemberRole.Professor);
    }

    public async Task<List<GroupMember>> GetProfessorsOfGroup(Guid groupId)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.GroupId == groupId && gm.Role == GroupMemberRole.Professor)
            .Include(gm => gm.Member).ThenInclude(m => m.User)
            .OrderBy(gm => gm.Member.LastName).ThenBy(gm => gm.Member.FirstName)
            .ToListAsync();
    }
}
