using Application.Interfaces.Services.Users;
using Application.Services.Groups;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.Join;

public class JoinGroupEndpoint : Endpoint<JoinGroupRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public JoinGroupEndpoint(
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
        Post("social/groups/join");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(JoinGroupRequest req, CancellationToken ct)
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
            await _groupService.JoinByInviteCode(req.InviteCode, member.Id);
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.InnerException?.Message
                ?? ex.InnerException?.Message
                ?? ex.Message;
            Logger.LogError(ex, "JoinGroup failed");
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("JoinFailed", innerMsg)), ct);
        }
    }
}
