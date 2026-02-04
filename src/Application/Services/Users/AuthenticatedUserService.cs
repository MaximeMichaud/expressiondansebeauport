using Application.Interfaces.Services;
using Application.Interfaces.Services.Users;
using Application.Services.Users.Exceptions;
using Domain.Entities.Identity;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Users;

public class AuthenticatedUserService : IAuthenticatedUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextUserService _httpContextUserService;

    public AuthenticatedUserService(
        IUserRepository userRepository,
        IHttpContextUserService httpContextUserService)
    {
        _userRepository = userRepository;
        _httpContextUserService = httpContextUserService;
    }

    public User? GetAuthenticatedUser()
    {
        return _userRepository.FindByEmail(_httpContextUserService.Username!);
    }

    public async Task<IdentityResult> ChangeUserPassword(string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            throw new ChangeAuthenticatedUserPasswordException("Current and new password cannot be null");

        var currentUserEmail = _httpContextUserService.Username;
        var user = _userRepository.FindByEmail(currentUserEmail!);
        if (user == null)
            throw new ChangeAuthenticatedUserPasswordException($"Could not find user with email {currentUserEmail}");

        return await _userRepository.UpdateUserPassword(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> ChangeUserEmail(string newEmail)
    {
        var user = GetAuthenticatedUser();
        if (user == null)
            throw new ChangeAuthenticatedUserEmailException($"Could not find user with email {_httpContextUserService.Username}");

        return await _userRepository.UpdateUserEmail(user, newEmail);
    }
}