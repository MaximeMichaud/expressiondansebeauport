using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.JoinRequests.Mine;

public class GetMyJoinRequestEndpoint : Endpoint<GetMyJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetMyJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/{GroupId}/join-requests/mine");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMyJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        var pending = await _joinRequestService.GetPendingRequest(req.GroupId, member.Id);
        if (pending == null)
        {
            await Send.OkAsync(new { found = false }, ct);
            return;
        }

        await Send.OkAsync(new
        {
            found = true,
            joinRequestId = pending.Id,
            status = pending.Status.ToString()
        }, ct);
    }
}
