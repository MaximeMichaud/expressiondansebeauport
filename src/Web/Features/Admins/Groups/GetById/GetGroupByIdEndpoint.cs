using Application.Services.Groups;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Groups.GetById;

public class GetGroupByIdEndpoint : Endpoint<GetGroupByIdRequest>
{
    private readonly IGroupService _groupService;
    private readonly IGroupMemberRepository _groupMemberRepository;

    public GetGroupByIdEndpoint(IGroupService groupService, IGroupMemberRepository groupMemberRepository)
    {
        _groupService = groupService;
        _groupMemberRepository = groupMemberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/groups/{Id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetGroupByIdRequest req, CancellationToken ct)
    {
        var group = await _groupService.GetGroupById(req.Id);
        if (group == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var members = await _groupMemberRepository.GetMembersOfGroup(group.Id);
        await Send.OkAsync(new
        {
            group.Id,
            group.Name,
            group.Description,
            group.ImageUrl,
            group.InviteCode,
            group.Season,
            group.IsArchived,
            Members = members.Select(m => new
            {
                m.Id,
                m.MemberId,
                m.Member.FirstName,
                m.Member.LastName,
                m.Member.ProfileImageUrl,
                Role = m.Role.ToString(),
                m.JoinedAt
            })
        }, ct);
    }
}
