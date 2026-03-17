using Application.Services.Groups;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.Archive;

public class ArchiveSeasonEndpoint : Endpoint<ArchiveSeasonRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;

    public ArchiveSeasonEndpoint(IGroupService groupService) => _groupService = groupService;

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/groups/archive");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ArchiveSeasonRequest req, CancellationToken ct)
    {
        await _groupService.ArchiveSeason(req.Season);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
