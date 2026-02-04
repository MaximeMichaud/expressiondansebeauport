using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Repositories.Users.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories.Users;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<Role> _roleManager;

    public RoleRepository(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Role> FindByName(string name)
    {
        var role = await _roleManager.FindByNameAsync(name);
        if (role == null)
            throw new RoleNotFoundException($"Could not find role with name {name} in database.");
        return role;
    }

    public void Dispose()
    {
        _roleManager.Dispose();
    }
}