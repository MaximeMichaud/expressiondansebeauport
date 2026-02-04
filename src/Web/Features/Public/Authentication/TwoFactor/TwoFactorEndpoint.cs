using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Web.Cookies;
using Web.Features.Common;

namespace Web.Features.Public.Authentication.TwoFactor;

public class TwoFactorEndpoint : EndpointWithSanitizedRequest<TwoFactorRequest, SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;

    public TwoFactorEndpoint(
        IUserRepository userRepository,
        IOptions<CookieSettings> cookieSettings,
        IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _cookieSettings = cookieSettings.Value;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("authentication/two-factor");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TwoFactorRequest req, CancellationToken ct)
    {
        var user = _userRepository.FindByUserName(req.Username);
        if (user == null)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false), ct);
            return;
        }

        var result = await _authenticationService.SignInUserWithTwoFactorCode(req.Code);
        if (!result.Succeeded)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false), ct);
            return;
        }

        user.UpdateLastTwoFactorAuthentication();
        await _userRepository.UpdateUser(user);

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