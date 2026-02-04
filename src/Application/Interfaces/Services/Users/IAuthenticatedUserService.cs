using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Services.Users;

public interface IAuthenticatedUserService
{
    User? GetAuthenticatedUser();
    Task<IdentityResult> ChangeUserPassword(string currentPassword, string newPassword);
    Task<IdentityResult> ChangeUserEmail(string newEmail);
}