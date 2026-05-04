using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.Delete;

public class DeleteGroupRequest
{
    public Guid Id { get; set; }
}

public class DeleteGroupEndpoint : Endpoint<DeleteGroupRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public DeleteGroupEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/groups/{Id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteGroupRequest req, CancellationToken ct)
    {
        try
        {
            await _groupService.DeleteGroup(req.Id);
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("DeleteGroup", ex.Message)), ct);
        }
    }
}
