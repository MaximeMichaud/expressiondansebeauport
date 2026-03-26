using Application.Services.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.Seasons;

public class GetSeasonsEndpoint : EndpointWithoutRequest
{
    private readonly IGroupService _groupService;

    public GetSeasonsEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/groups/seasons");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var seasons = await _groupService.GetDistinctSeasons();
        await Send.OkAsync(seasons, ct);
    }
}
