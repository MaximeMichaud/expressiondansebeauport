using Application.Exceptions.Members;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Members;

public class MemberRepository : IMemberRepository
{
    private readonly GarneauTemplateDbContext _context;

    public MemberRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public PaginatedList<Member> GetAllPaginated(int pageIndex, int pageSize)
    {
        var query = _context.Members
            .Include(x => x.User)
            .AsNoTracking();
        var pageItems = query.OrderByDescending(x => x.Created).Skip((pageIndex-1) * pageSize).Take(pageSize);
        return new PaginatedList<Member>(pageItems.ToList(), query.Count());
    }

    public Member FindById(Guid id)
    {
        var member = _context.Members
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.Id == id);
        if (member == null)
            throw new MemberNotFoundException($"No member with id {id} was found.");
        return member;
    }

    public Member? FindByUserId(Guid userId, bool asNoTracking = true)
    {
        var query = _context.Members as IQueryable<Member>;
        if (asNoTracking)
            query = query.AsNoTracking();
        return query
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.User.Id == userId);
    }

    public Member? FindByUserEmail(string userEmail)
    {
        return _context.Members
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.User.Email == userEmail);
    }

    public async Task Create(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Member member)
    {
        if (!_context.Members.Any(x => x.Id == member.Id))
            throw new MemberNotFoundException($"Could not find member with id {member.Id}.");

        _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }
}