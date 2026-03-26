using Domain.Entities;

namespace Domain.Repositories;

public interface IAdministratorRepository
{
    Administrator? FindByUserId(Guid userId, bool asNoTracking = true);
    Task<List<Administrator>> GetAll();
    Task Update(Administrator admin);
}