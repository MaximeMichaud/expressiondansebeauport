using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.Create;

public class CreateGroupEndpoint : Endpoint<CreateGroupRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public CreateGroupEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/groups");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateGroupRequest req, CancellationToken ct)
    {
        await _groupService.CreateGroup(req.Name, req.Description, req.Season, req.InviteCode, req.ImageUrl);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
