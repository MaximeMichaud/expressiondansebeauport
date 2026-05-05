using Application.Interfaces.Services.Users;
using Application.Settings;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Web.Cookies;

namespace Web.Features.Public.Authentication.Logout;

public class LogoutEndpoint : EndpointWithoutRequest<SucceededOrNotResponse>
{
    private readonly CookieSettings _cookieSettings;
    private readonly IAuthenticationService _authenticationService;

    public LogoutEndpoint(
        IOptions<CookieSettings> cookieSettings,
        IAuthenticationService authenticationService)
    {
        _cookieSettings = cookieSettings.Value;
        _authenticationService = authenticationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("authentication/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentRefreshToken = HttpContext.GetCookieValue(CookieName.REFRESH);
        if (!string.IsNullOrWhiteSpace(currentRefreshToken))
            await _authenticationService.DeleteRefreshToken(currentRefreshToken);

        HttpContext.Response.ClearAuthCookies(_cookieSettings.Domain, _cookieSettings.Secure);

        await Send.OkAsync(new SucceededOrNotResponse(succeeded: true), ct);
    }
}