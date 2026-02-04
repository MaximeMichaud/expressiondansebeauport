using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Admins;

public class AdministratorRepository : IAdministratorRepository
{
    private readonly GarneauTemplateDbContext _context;

    public AdministratorRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public Administrator? FindByUserId(Guid userId, bool asNoTracking = true)
    {
        var query = _context.Administrators as IQueryable<Administrator>;
        if (asNoTracking)
            query = query.AsNoTracking();
        return query
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.User.Id == userId);
    }
}