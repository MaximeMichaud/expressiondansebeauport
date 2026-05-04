using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Members;

public class MemberRepository : IMemberRepository
{
    private readonly GarneauTemplateDbContext _context;

    public MemberRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
    }

    public Member? FindByUserId(Guid userId, bool asNoTracking = true)
    {
        var query = _context.Members as IQueryable<Member>;
        if (asNoTracking) query = query.AsNoTracking();
        return query
            .Include(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(m => m.UserId == userId);
    }

    public Member? FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.Members as IQueryable<Member>;
        if (asNoTracking) query = query.AsNoTracking();
        return query
            .Include(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(m => m.Id == id);
    }

    public async Task<List<Member>> Search(string? search, int skip, int take)
    {
        var query = _context.Members
            .Include(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.User.Email!.ToLower().Contains(term));
        }

        return await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task<int> Count(string? search)
    {
        var query = _context.Members.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.User.Email!.ToLower().Contains(term));
        }

        return await query.CountAsync();
    }

    public async Task Update(Member member)
    {
        if (_context.Entry(member).State == EntityState.Detached)
            _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Member>> GetAdminMembers()
    {
        return await _context.Members
            .AsNoTracking()
            .Include(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(m => m.User.UserRoles.Any(ur => ur.Role.NormalizedName == "ADMIN"))
            .ToListAsync();
    }
}
