using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.JoinRequests;

public class JoinRequestRepository : IJoinRequestRepository
{
    private readonly GarneauTemplateDbContext _context;

    public JoinRequestRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(JoinRequest joinRequest)
    {
        _context.JoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync();
    }

    public async Task<JoinRequest?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.JoinRequests
            .Include(jr => jr.Group)
            .Include(jr => jr.RequesterMember)
            .Include(jr => jr.ResolvedByMember)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(jr => jr.Id == id);
    }

    public async Task<JoinRequest?> FindPendingByGroupAndMember(Guid groupId, Guid memberId)
    {
        return await _context.JoinRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(jr =>
                jr.GroupId == groupId &&
                jr.RequesterMemberId == memberId &&
                jr.Status == JoinRequestStatus.Pending);
    }

    public async Task Update(JoinRequest joinRequest)
    {
        if (_context.Entry(joinRequest).State == EntityState.Detached)
            _context.JoinRequests.Update(joinRequest);
        await _context.SaveChangesAsync();
    }
}
