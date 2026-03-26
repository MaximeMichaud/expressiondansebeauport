using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.Update;

public class UpdateGroupEndpoint : Endpoint<UpdateGroupRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public UpdateGroupEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Put("admin/groups/{Id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateGroupRequest req, CancellationToken ct)
    {
        await _groupService.UpdateGroup(req.Id, req.Name, req.Description, req.Season, req.ImageUrl);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
