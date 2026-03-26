using Application.Services.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.Active;

public class ActiveGroupsEndpoint : EndpointWithoutRequest
{
    private readonly IGroupService _groupService;

    public ActiveGroupsEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/active");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var groups = await _groupService.GetActiveGroups();
        await Send.OkAsync(groups.Select(g => new
        {
            g.Id,
            g.Name,
            g.Description,
            g.ImageUrl,
            g.Season,
            g.InviteCode,
            MemberCount = g.Members.Count
        }), ct);
    }
}
