using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Services.Users;

public interface IAuthenticationService
{
    Task<User?> FindUserWithUsernameAndPassword(string username, string password);
    Task<string?> GetTwoFactorAuthenticationTokenCodeUserWithPassword(User user, string password);
    Task<SignInResult> SignInUserWithTwoFactorCode(string code);

    string CreateJwtAccessToken(User user);
    Task<string> CreateRefreshToken(User user);
    Task<bool> ValidateRefreshToken(string token);
    Task DeleteRefreshToken(string token);
}