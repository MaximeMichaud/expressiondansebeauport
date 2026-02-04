using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using Domain.Extensions;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Web.Cookies;

namespace Web.Features.Public.Authentication.RefreshToken;

public class RefreshTokenEndpoint : EndpointWithoutRequest<SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenEndpoint(
        IUserRepository userRepository,
        IOptions<CookieSettings> cookieSettings,
        IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _cookieSettings = cookieSettings.Value;
        _authenticationService = authenticationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("authentication/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentAccessToken = HttpContext.GetCookieValue(CookieName.ACCESS);
        var username = currentAccessToken.GetUsernameFromJwtToken();

        if (string.IsNullOrWhiteSpace(username))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var user = _userRepository.FindByUserName(username);
        if (user == null)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var currentRefreshToken = HttpContext.GetCookieValue(CookieName.REFRESH);
        if (string.IsNullOrWhiteSpace(currentRefreshToken) ||
            !await _authenticationService.ValidateRefreshToken(currentRefreshToken))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        HttpContext.Response.SetCookieValue(
            CookieName.ACCESS,
            _authenticationService.CreateJwtAccessToken(user),
            _cookieSettings.Domain,
            _cookieSettings.Secure,
            false);

        HttpContext.Response.SetCookieValue(
            CookieName.REFRESH,
            await _authenticationService.CreateRefreshToken(user),
            _cookieSettings.Domain,
            _cookieSettings.Secure,
            true);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}