using Application.Interfaces.Services.Admins;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Me.UpdateMe;

public class UpdateMeEndpoint : Endpoint<UpdateMeRequest, SucceededOrNotResponse>
{
    private readonly IAuthenticatedAdminService _authenticatedAdminService;

    public UpdateMeEndpoint(IAuthenticatedAdminService authenticatedAdminService)
    {
        _authenticatedAdminService = authenticatedAdminService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("admins/me");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMeRequest req, CancellationToken ct)
    {
        await _authenticatedAdminService.UpdateAdmin(req.FirstName, req.LastName, req.Email);
        await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
    }
}
