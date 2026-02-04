using Domain.Entities.Identity;

namespace Domain.Repositories;

public interface IRoleRepository : IDisposable
{
    Task<Role> FindByName(string name);
}