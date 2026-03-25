using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Groups;

public class GroupRepository : IGroupRepository
{
    private readonly GarneauTemplateDbContext _context;

    public GroupRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
    }

    public async Task<Group?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.Groups
            .Include(g => g.Members).ThenInclude(gm => gm.Member).ThenInclude(m => m.User)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group?> FindByInviteCode(string code)
    {
        return await _context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.InviteCode == code && !g.IsArchived);
    }

    public async Task<List<Group>> GetBySeason(string season, bool includeArchived = false)
    {
        var query = _context.Groups.AsNoTracking().Where(g => g.Season == season);
        if (!includeArchived)
            query = query.Where(g => !g.IsArchived);
        return await query.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<List<Group>> GetActive()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .AsNoTracking()
            .Where(g => !g.IsArchived)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<List<Group>> GetAll()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .AsNoTracking()
            .OrderByDescending(g => g.Created)
            .ToListAsync();
    }

    public async Task Update(Group group)
    {
        if (_context.Entry(group).State == EntityState.Detached)
            _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetDistinctSeasons()
    {
        return await _context.Groups
            .AsNoTracking()
            .Select(g => g.Season)
            .Distinct()
            .OrderByDescending(s => s)
            .ToListAsync();
    }
}
