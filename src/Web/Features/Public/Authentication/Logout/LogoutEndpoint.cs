using Application.Interfaces.Services.Users;
using Application.Interfaces.Services;
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
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IAuditLogService _auditLogService;

    public LogoutEndpoint(
        IOptions<CookieSettings> cookieSettings,
        IAuthenticationService authenticationService,
        IAuthenticatedUserService authenticatedUserService,
        IAuditLogService auditLogService)
    {
        _cookieSettings = cookieSettings.Value;
        _authenticationService = authenticationService;
        _authenticatedUserService = authenticatedUserService;
        _auditLogService = auditLogService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("authentication/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authenticatedUser = _authenticatedUserService.GetAuthenticatedUser();
        var currentRefreshToken = HttpContext.GetCookieValue(CookieName.REFRESH);
        if (!string.IsNullOrWhiteSpace(currentRefreshToken))
            await _authenticationService.DeleteRefreshToken(currentRefreshToken);

        HttpContext.Response.ClearAuthCookies(_cookieSettings.Domain, _cookieSettings.Secure);

        if (authenticatedUser?.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR) == true)
            await _auditLogService.LogAsync("logout", "admin-session", authenticatedUser.Id, "Déconnexion admin.", authenticatedUser.Id, null, authenticatedUser.Email);

        await Send.OkAsync(new SucceededOrNotResponse(succeeded: true), ct);
    }
}
