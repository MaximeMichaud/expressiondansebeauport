using Application.Exceptions.Admins;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Admins;
using Application.Interfaces.Services.Users;
using Application.Services.Admins.Exceptions;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services.Admins;

public class AuthenticatedAdminService : IAuthenticatedAdminService
{
    private readonly IAdministratorRepository _administratorRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IUserRepository _userRepository;

    public AuthenticatedAdminService(
        IAdministratorRepository administratorRepository,
        IAuthenticatedUserService authenticatedUserService,
        IUserRepository userRepository)
    {
        _administratorRepository = administratorRepository;
        _authenticatedUserService = authenticatedUserService;
        _userRepository = userRepository;
    }

    public Administrator GetAuthenticatedAdmin()
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null)
            throw new UserNotFoundException("Could not find user associated with authenticated admin.");

        if (user.UserRoles.Count == 0)
            throw new GetAuthenticatedAdminException($"Authenticated user {user.Id} has no role.");

        var member = _administratorRepository.FindByUserId(user.Id);
        if (member == null)
            throw new AdministratorNotFoundException($"Could not find admin associated with user with id {user.Id}");

        return member;
    }

    public async Task UpdateAdmin(string firstName, string lastName, string email)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null)
            throw new UserNotFoundException("Could not find user associated with authenticated admin.");

        var admin = _administratorRepository.FindByUserId(user.Id, asNoTracking: false);
        if (admin == null)
            throw new AdministratorNotFoundException($"Could not find admin associated with user with id {user.Id}");

        var existingUser = _userRepository.FindByEmail(email);
        if (existingUser != null && existingUser.Id != user.Id && existingUser.IsActive())
            throw new UserWithEmailAlreadyExistsException("A user with this email already exists.");

        admin.SetFirstName(firstName);
        admin.SetLastName(lastName);

        admin.User.Email = email.ToLowerInvariant();
        admin.User.UserName = email.ToLowerInvariant();
        admin.User.NormalizedEmail = email.ToUpperInvariant();
        admin.User.NormalizedUserName = email.ToUpperInvariant();

        admin.SanitizeForSaving();
        await _administratorRepository.Update(admin);
    }
}