using Domain.Entities;

namespace Domain.Repositories;

public interface IAdministratorRepository
{
    Administrator? FindByUserId(Guid userId, bool asNoTracking = true);
}