using Application.Interfaces.Services.Notifications;
using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Web.Cookies;
using Web.Features.Common;

namespace Web.Features.Public.Authentication.Login;

public class LoginEndpoint : EndpointWithSanitizedRequest<LoginRequest, SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly INotificationService _notificationService;
    private readonly IAuthenticationService _authenticationService;

    public LoginEndpoint(
        IOptions<CookieSettings> cookieSettings,
        INotificationService notificationService,
        IAuthenticationService authenticationService)
    {
        _cookieSettings = cookieSettings.Value;
        _notificationService = notificationService;
        _authenticationService = authenticationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("authentication/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _authenticationService.FindUserWithUsernameAndPassword(req.Username, req.Password);
        if (user == null)
        {
            await Send.OkAsync(new SucceededOrNotResponse(succeeded: false), ct);
            return;
        }

        var code = await _authenticationService.GetTwoFactorAuthenticationTokenCodeUserWithPassword(user, req.Password);
        if (code == null)
        {
            await CreateAccessAndRefreshTokenForUser(user);
            await Send.OkAsync(new SucceededOrNotResponse(succeeded: true), ct);
            return;
        }

        await _notificationService.SendTwoFactorAuthenticationCodeNotification(user, code);
        var error = new Error("TwoFactorRequired", "Next step: complete two factor authentication.");
        await Send.OkAsync(new SucceededOrNotResponse(succeeded: false, error), ct);
    }

    private async Task CreateAccessAndRefreshTokenForUser(User user)
    {
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
    }
}