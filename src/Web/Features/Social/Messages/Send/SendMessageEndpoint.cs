using Application.Interfaces.Services.Users;
using Application.Services.Messaging;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Features.Social.Messages.Send;

public class SendMessageEndpoint : Endpoint<SendMessageRequest, SucceededOrNotResponse>
{
    private readonly IConversationService _conversationService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public SendMessageEndpoint(
        IConversationService conversationService,
        IConversationRepository conversationRepository,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IHubContext<ChatHub> hubContext)
    {
        _conversationService = conversationService;
        _conversationRepository = conversationRepository;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/conversations/{ConversationId}/messages");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var media = req.Media.Select(m => new MessageMediaItem
        {
            DisplayUrl = m.DisplayUrl,
            ThumbnailUrl = m.ThumbnailUrl,
            OriginalUrl = m.OriginalUrl,
            ContentType = m.ContentType,
            Size = m.Size
        }).ToList();

        Domain.Entities.Message message;
        try
        {
            message = await _conversationService.SendMessage(
                req.ConversationId,
                member.Id,
                req.Content,
                media);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidMessage", ex.Message)), ct);
            return;
        }

        var conversation = await _conversationRepository.FindById(req.ConversationId);
        var recipientMemberId = conversation?.ParticipantAMemberId == member.Id
            ? conversation?.ParticipantBMemberId
            : conversation?.ParticipantAMemberId;
        var recipientUser = recipientMemberId.HasValue ? _memberRepository.FindById(recipientMemberId.Value) : null;
        if (recipientUser != null)
        {
            var connectionId = ChatHub.GetConnectionId(recipientUser.UserId.ToString());
            if (connectionId != null)
            {
                var mediaPayload = message.Media
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new
                    {
                        m.Id,
                        m.MediaUrl,
                        m.ThumbnailUrl,
                        m.OriginalUrl,
                        m.ContentType,
                        m.Size,
                        m.SortOrder
                    })
                    .ToList();

                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.Content,
                    SenderName = member.FullName,
                    message.ConversationId,
                    Media = mediaPayload
                }, ct);
            }
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
