using Application.Exceptions.Users;
using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Repositories.Users.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public bool UserWithEmailExists(string email)
    {
        var user = _userManager.Users.FirstOrDefault(x => x.Email!.ToLower() == email.ToLower());
        return user != null && user.IsActive();
    }

    public User? FindById(Guid id)
    {
        return _userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => !x.Deleted.HasValue)
            .FirstOrDefault(x => x.Id == id);
    }

    public User? FindByUserName(string userName)
    {
        var user = _userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.UserName!.ToLower() == userName.ToLower());
        return user != null && user.IsActive() ? user : null;
    }

    public User? FindByEmail(string email, bool includeDeleted = false)
    {
        var user = _userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefault(x => x.Email!.ToLower() == email.ToLower());
        if (includeDeleted)
            return user;
        return user != null && user.IsActive() ? user : null;
    }

    public async Task<User> SyncUser(User user)
    {
        if (!_userManager.Users.Any(x => x.Email == user.Email))
            return await CreateUser(user);
        return await UpdateUser(user);
    }

    public async Task<User> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || !user.RoleNames.Any())
            throw new CreateUserException("Could not create user since role or email is null.");

        if (_userManager.Users.Any(x => x.Email == user.Email))
            throw new UserWithEmailAlreadyExistsException($"User with email {user.Email} already exists.");

        await _userManager.CreateAsync(user);
        await _userManager.AddToRolesAsync(user, user.RoleNames);

        return (await _userManager.FindByEmailAsync(user.Email))!;
    }

    public async Task<User> UpdateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || !user.RoleNames.Any())
            throw new UpdateUserException("Could not update user either because user role or email is null.");

        if (!_userManager.Users.Any(x => x.Id == user.Id))
            throw new UserNotFoundException($"User with id {user.Id} does not exist.");

        if (_userManager.Users.Any(x => x.Email == user.Email && x.Id != user.Id))
            throw new UserWithEmailAlreadyExistsException("Could not change this user's email since another user has same email.");

        var newRoles = user.RoleNames;
        var roles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, roles);
        await _userManager.AddToRolesAsync(user, newRoles);
        await _userManager.UpdateAsync(user);

        return (await _userManager.FindByEmailAsync(user.Email))!;
    }

    public async Task<IdentityResult> UpdateUserEmail(User user, string newEmail)
    {
        user.Email = newEmail;
        user.UserName = newEmail;
        user.EmailConfirmed = true;
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUser(User user)
    {
        if (await _userManager.FindByIdAsync(user.Id.ToString()) == null)
            throw new UserNotFoundException($"User with id {user.Id} does not exist.");

        user.SoftDelete();
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserWithId(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new UserNotFoundException($"User with id {id} does not exist.");
        user.SoftDelete();
        return await _userManager.UpdateAsync(user);
    }

    public async Task<string> GetResetPasswordTokenForUser(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetUserPassword(User user, string password, string resetPasswordToken)
    {
        return await _userManager.ResetPasswordAsync(user, resetPasswordToken, password);
    }

    public async Task<IdentityResult> CreateUserPassword(User user, string password)
    {
        return await _userManager.AddPasswordAsync(user, password);
    }

    public async Task<IdentityResult> UpdateUserPassword(User user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public void Dispose()
    {
        _userManager.Dispose();
    }
}