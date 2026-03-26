using Application.Interfaces.Services.Users;
using Application.Services.Groups;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.MyGroups;

public class MyGroupsEndpoint : EndpointWithoutRequest
{
    private readonly IGroupService _groupService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public MyGroupsEndpoint(
        IGroupService groupService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _groupService = groupService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/mine");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var groups = await _groupService.GetGroupsForMember(member.Id);
        await Send.OkAsync(groups.Select(g => new
        {
            g.Id,
            g.Name,
            g.Description,
            g.ImageUrl,
            g.Season,
            g.IsArchived,
            MemberCount = g.Members.Count
        }), ct);
    }
}
