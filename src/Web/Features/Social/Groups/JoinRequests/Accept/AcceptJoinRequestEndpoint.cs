using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Accept;

public class AcceptJoinRequestEndpoint : Endpoint<AcceptJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public AcceptJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IHubContext<ChatHub> hubContext)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/join-requests/{Id}/accept");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AcceptJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        try
        {
            await _joinRequestService.AcceptRequest(req.Id, member.Id);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Error", ex.Message)), ct);
            return;
        }

        var joinRequest = await _joinRequestService.GetJoinRequestById(req.Id);
        if (joinRequest != null)
        {
            var requesterMember = _memberRepository.FindById(joinRequest.RequesterMemberId);
            if (requesterMember != null)
            {
                var connectionId = ChatHub.GetConnectionId(requesterMember.UserId.ToString());
                if (connectionId != null)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Accepted",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }

            var professors = await _joinRequestService.GetProfessorsForGroup(joinRequest.GroupId);
            foreach (var prof in professors)
            {
                if (prof.MemberId == member.Id) continue;
                var profMember = _memberRepository.FindById(prof.MemberId);
                if (profMember == null) continue;
                var connId = ChatHub.GetConnectionId(profMember.UserId.ToString());
                if (connId != null)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("JoinRequestResolved", new
                    {
                        JoinRequestId = joinRequest.Id,
                        Status = "Accepted",
                        ResolvedByName = member.FullName,
                        joinRequest.GroupId,
                        GroupName = joinRequest.Group?.Name
                    }, ct);
                }
            }
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
