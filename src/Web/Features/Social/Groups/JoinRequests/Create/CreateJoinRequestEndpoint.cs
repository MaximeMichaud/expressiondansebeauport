using Application.Interfaces.Services.Users;
using Application.Services.JoinRequests;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Groups.JoinRequests.Create;

public class CreateJoinRequestEndpoint : Endpoint<CreateJoinRequestRequest>
{
    private readonly IJoinRequestService _joinRequestService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public CreateJoinRequestEndpoint(
        IJoinRequestService joinRequestService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IGroupRepository groupRepository,
        IHubContext<ChatHub> hubContext)
    {
        _joinRequestService = joinRequestService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _groupRepository = groupRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/groups/{GroupId}/join-requests");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateJoinRequestRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        Domain.Entities.JoinRequest joinRequest;
        try
        {
            joinRequest = await _joinRequestService.CreateRequest(req.GroupId, member.Id);
        }
        catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE")
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Duplicate", "Une demande est déjà en attente.")), ct);
            return;
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("Error", ex.Message)), ct);
            return;
        }

        var group = await _groupRepository.FindById(req.GroupId);
        var professors = await _joinRequestService.GetProfessorsForGroup(req.GroupId);

        foreach (var prof in professors)
        {
            var profMember = _memberRepository.FindById(prof.MemberId);
            if (profMember == null) continue;
            var connectionId = ChatHub.GetConnectionId(profMember.UserId.ToString());
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    Id = Guid.NewGuid(),
                    Content = $"{member.FullName} souhaite rejoindre le groupe {group?.Name ?? ""}",
                    SenderName = member.FullName,
                    ConversationId = Guid.Empty,
                    MessageType = "JoinRequest",
                    JoinRequestId = joinRequest.Id,
                    JoinRequestStatus = "Pending",
                    GroupName = group?.Name,
                    RequesterName = member.FullName,
                    Media = Array.Empty<object>()
                }, ct);
            }
        }

        await Send.OkAsync(new { succeeded = true, joinRequestId = joinRequest.Id }, ct);
    }
}
