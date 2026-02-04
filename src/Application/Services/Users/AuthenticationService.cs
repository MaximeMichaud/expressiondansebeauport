using System.Security.Cryptography;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Users;
using Application.Services.Users.Exceptions;
using Application.Settings;
using Domain.Authentication;
using Domain.Entities.Authentication;
using Domain.Entities.Identity;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Services.Users;

public class AuthenticationService : IAuthenticationService
{
    private readonly int _twoFactorAuthenticationDayDelay;

    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtTokenSettings _jwtTokenSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthenticationService(
        IUserRepository userRepository,
        SignInManager<User> signInManager,
        IOptions<JwtTokenSettings> jwtTokenSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<ApplicationSettings> applicationSettings)
    {
        _signInManager = signInManager;
        _userRepository = userRepository;
        _jwtTokenSettings = jwtTokenSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _twoFactorAuthenticationDayDelay = applicationSettings.Value.TwoFactorAuthenticationDayDelay;
    }

    public async Task<User?> FindUserWithUsernameAndPassword(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = _userRepository.FindByUserName(username);
        if (user == null)
            return null;

        return (await _signInManager.CheckPasswordSignInAsync(user, password, false)).Succeeded ? user : null;
    }

    public async Task<string?> GetTwoFactorAuthenticationTokenCodeUserWithPassword(User user, string password)
    {
        var twoFactorAuthDoneRecently =
            user.LastTwoFactorAuthenticationWasLessThanGivenNumberOfDaysAgo(_twoFactorAuthenticationDayDelay);
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

        if (result == SignInResult.Failed || result == SignInResult.LockedOut || result == SignInResult.NotAllowed)
            throw new TwoFactorAuthenticationException($"Could not get 2fa code for user with email {user.Email}.");

        if (result == SignInResult.Success || result == SignInResult.TwoFactorRequired && twoFactorAuthDoneRecently)
            return null;

        return await _signInManager.UserManager.GenerateTwoFactorTokenAsync(user, "Email");
    }

    public async Task<SignInResult> SignInUserWithTwoFactorCode(string code)
    {
        return await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
    }

    public string CreateJwtAccessToken(User user)
    {
        if (user == null)
            throw new UserNotFoundException("Can't create JWT access token for null user.");

        if (string.IsNullOrWhiteSpace(_jwtTokenSettings.SecretKey))
            throw new InvalidJwtSettingsException("The JWT secret key cannot be empty. Please check your appsettings.");

        return JwtTokenFactory.CreateToken(
            user,
            _jwtTokenSettings.SecretKey,
            DateTime.UtcNow.AddMinutes(_jwtTokenSettings.TokenExpiry),
            _jwtTokenSettings.Issuer,
            _jwtTokenSettings.Audience);
    }

    public async Task<string> CreateRefreshToken(User user)
    {
        if (user == null)
            throw new UserNotFoundException("Can't refresh token for null user.");

        var refreshToken = new RefreshToken(user, await GenerateUniqueToken());
        await _refreshTokenRepository.Create(refreshToken);

        return refreshToken.Token;
    }

    public async Task<bool> ValidateRefreshToken(string token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               await _refreshTokenRepository.ValidRefreshTokenWithTokenExists(token);
    }

    public async Task DeleteRefreshToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        await _refreshTokenRepository.DeleteRefreshTokenWithToken(token);
    }

    private async Task<string> GenerateUniqueToken()
    {
        string token;

        do
        {
            token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        } while (await _refreshTokenRepository.RefreshTokenWithTokenExists(token));

        return token;
    }
}