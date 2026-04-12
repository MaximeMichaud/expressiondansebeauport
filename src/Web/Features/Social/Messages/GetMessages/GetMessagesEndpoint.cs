using Application.Interfaces.Services.Users;
using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Messages.GetMessages;

public class GetMessagesEndpoint : Endpoint<GetMessagesRequest>
{
    private readonly IConversationService _conversationService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetMessagesEndpoint(
        IConversationService conversationService,
        IConversationRepository conversationRepository,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _conversationService = conversationService;
        _conversationRepository = conversationRepository;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/conversations/{ConversationId}/messages");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.OkAsync(new List<object>(), ct);
            return;
        }

        var messagesResult = await _conversationService.GetMessages(req.ConversationId, req.Page);
        var messages = messagesResult.Items;

        // Get the other participant's LastReadAt to determine read receipts
        var conversation = await _conversationRepository.FindById(req.ConversationId);
        var otherParticipant = conversation?.Participants?.FirstOrDefault(p => p.MemberId != member.Id);
        var otherLastReadAt = otherParticipant?.LastReadAt;

        var results = messages.Select(m =>
        {
            // Build media array: prefer the new MessageMedia collection; fall back to legacy single-media columns.
            var mediaList = m.Media?
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    MediaUrl = x.MediaUrl,
                    ThumbnailUrl = x.ThumbnailUrl,
                    OriginalUrl = x.OriginalUrl,
                    x.ContentType,
                    x.Size,
                    x.SortOrder
                })
                .Cast<object>()
                .ToList() ?? new List<object>();

            if (mediaList.Count == 0 && !string.IsNullOrEmpty(m.MediaUrl))
            {
                mediaList.Add(new
                {
                    Id = Guid.Empty,
                    MediaUrl = m.MediaUrl,
                    ThumbnailUrl = m.MediaThumbnailUrl,
                    OriginalUrl = m.MediaOriginalUrl,
                    ContentType = "image/webp",
                    Size = 0L,
                    SortOrder = 0
                });
            }

            return new
            {
                m.Id,
                m.ConversationId,
                SenderMemberId = m.SenderMemberId,
                SenderName = m.SenderMember?.FullName ?? "Inconnu",
                Content = m.Deleted.HasValue ? null : m.Content,
                Media = m.Deleted.HasValue ? new List<object>() : mediaList,
                Created = m.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                IsDeleted = m.Deleted.HasValue,
                IsRead = m.SenderMemberId == member.Id && otherLastReadAt != null && m.Created <= otherLastReadAt,
                MessageType = m.MessageType.ToString(),
                JoinRequest = m.JoinRequest != null ? new
                {
                    m.JoinRequest.Id,
                    m.JoinRequest.GroupId,
                    GroupName = m.JoinRequest.Group?.Name,
                    RequesterMemberId = m.JoinRequest.RequesterMemberId,
                    RequesterName = m.JoinRequest.RequesterMember?.FullName ?? "Inconnu",
                    Status = m.JoinRequest.Status.ToString(),
                    ResolvedByName = m.JoinRequest.ResolvedByMember?.FullName
                } : null
            };
        });

        await Send.OkAsync(new { Items = results, messagesResult.HasMore }, ct);
    }
}
