using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Users.Me.ChangePassword;

public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest, SucceededOrNotResponse>
{
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public ChangePasswordEndpoint(IAuthenticatedUserService authenticatedUserService)
    {
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("users/me/change-password");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var identityResult = await _authenticatedUserService.ChangeUserPassword(req.CurrentPassword, req.NewPassword);
        if (!identityResult.Succeeded)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, identityResult.GetErrors()), ct);
            return;
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
