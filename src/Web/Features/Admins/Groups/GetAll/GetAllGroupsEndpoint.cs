using Application.Services.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.GetAll;

public class GetAllGroupsEndpoint : EndpointWithoutRequest
{
    private readonly IGroupService _groupService;

    public GetAllGroupsEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/groups");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var groups = await _groupService.GetAllGroups();
        await Send.OkAsync(groups.Select(g => new
        {
            g.Id,
            g.Name,
            g.Description,
            g.ImageUrl,
            g.InviteCode,
            g.Season,
            g.IsArchived,
            MemberCount = g.Members.Count
        }), ct);
    }
}
