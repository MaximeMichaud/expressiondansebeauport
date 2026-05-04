using Application.Interfaces.Services.Users;
using Application.Services.Groups;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.LeaveGroup;

public class LeaveGroupRequest
{
    public Guid GroupId { get; set; }
}

public class LeaveGroupEndpoint : Endpoint<LeaveGroupRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public LeaveGroupEndpoint(
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
        Delete("social/groups/{GroupId}/leave");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(LeaveGroupRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        try
        {
            await _groupService.LeaveGroup(req.GroupId, member.Id);
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("LeaveGroup", ex.Message)), ct);
        }
    }
}
