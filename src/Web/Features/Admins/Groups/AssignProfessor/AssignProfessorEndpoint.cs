using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.AssignProfessor;

public class AssignProfessorEndpoint : Endpoint<AssignProfessorRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public AssignProfessorEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/groups/{GroupId}/professors");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AssignProfessorRequest req, CancellationToken ct)
    {
        await _groupService.AssignProfessor(req.GroupId, req.MemberId);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
