using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.RemoveProfessor;

public class RemoveProfessorEndpoint : Endpoint<RemoveProfessorRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public RemoveProfessorEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/groups/{GroupId}/professors/{MemberId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(RemoveProfessorRequest req, CancellationToken ct)
    {
        await _groupService.RemoveProfessor(req.GroupId, req.MemberId);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
