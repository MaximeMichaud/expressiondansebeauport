using Application.Interfaces.Services.Users;
using Application.Services.Groups;
using Domain.Enums;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.Details;

public class GroupDetailsEndpoint : Endpoint<GroupDetailsRequest>
{
    private readonly IGroupService _groupService;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GroupDetailsEndpoint(
        IGroupService groupService,
        IGroupMemberRepository groupMemberRepository,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _groupService = groupService;
        _groupMemberRepository = groupMemberRepository;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/{Id}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GroupDetailsRequest req, CancellationToken ct)
    {
        var group = await _groupService.GetGroupById(req.Id);
        if (group == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);

        var isAdmin = user.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR);
        var isProfessorInGroup = false;

        if (member != null && !isAdmin)
        {
            var gm = await _groupMemberRepository.FindByGroupAndMember(group.Id, member.Id);
            isProfessorInGroup = gm?.Role == GroupMemberRole.Professor;
        }

        var showInviteCode = isAdmin || isProfessorInGroup;

        await Send.OkAsync(new
        {
            group.Id,
            group.Name,
            group.Description,
            group.ImageUrl,
            group.Season,
            group.IsArchived,
            InviteCode = showInviteCode ? group.InviteCode : null,
            MemberCount = group.Members.Count
        }, ct);
    }
}
