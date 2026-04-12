using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Reject;

public class RejectJoinRequestEndpoint : Endpoint<RejectJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public RejectJoinRequestEndpoint(
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
        Put("social/join-requests/{Id}/reject");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(RejectJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        try
        {
            await _joinRequestService.RejectRequest(req.Id, member.Id);
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
                await _hubContext.Clients.User(requesterMember.UserId.ToString()).SendAsync("ReceiveMessage", new
                {
                    Id = Guid.NewGuid(),
                    Content = $"Votre demande pour {joinRequest.Group?.Name ?? "le groupe"} a été refusée.",
                    SenderName = member.FullName,
                    ConversationId = Guid.Empty,
                    JoinRequestNotification = "Rejected",
                    GroupName = joinRequest.Group?.Name,
                    Media = Array.Empty<object>()
                }, ct);
            }
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
