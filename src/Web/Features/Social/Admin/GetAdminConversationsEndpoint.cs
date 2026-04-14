using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Admin;

public class GetAdminConversationsEndpoint : Endpoint<GetAdminConversationsRequest>
{
    private readonly IConversationService _conversationService;

    public GetAdminConversationsEndpoint(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/admin/members/{MemberId}/conversations");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAdminConversationsRequest req, CancellationToken ct)
    {
        var conversationsResult = await _conversationService.GetConversations(req.MemberId, req.Page);
        var conversations = conversationsResult.Items;

        var results = conversations.Select(c =>
        {
            var lastMsg = c.Messages?.OrderByDescending(m => m.Created).FirstOrDefault();
            var participant = c.Participants?.FirstOrDefault(p => p.MemberId == req.MemberId);
            var unread = 0;
            if (lastMsg != null && lastMsg.SenderMemberId != req.MemberId)
            {
                if (participant?.LastReadAt == null || lastMsg.Created > participant.LastReadAt)
                    unread = 1;
            }

            return new
            {
                c.Id,
                ParticipantA = new
                {
                    Id = c.ParticipantAMemberId,
                    FullName = c.ParticipantA?.FullName ?? "Inconnu",
                    ProfileImageUrl = c.ParticipantA?.ProfileImageUrl,
                    AvatarColor = c.ParticipantA?.AvatarColor ?? "#1a1a1a"
                },
                ParticipantB = new
                {
                    Id = c.ParticipantBMemberId,
                    FullName = c.ParticipantB?.FullName ?? "Inconnu",
                    ProfileImageUrl = c.ParticipantB?.ProfileImageUrl,
                    AvatarColor = c.ParticipantB?.AvatarColor ?? "#1a1a1a"
                },
                LastMessage = lastMsg != null ? new
                {
                    Content = lastMsg.Content,
                    SenderName = lastMsg.SenderMember?.FullName ?? "",
                    Created = lastMsg.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    MediaCount = lastMsg.Media?.Count ?? 0,
                    HasVideo = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("video/")),
                    HasImage = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("image/")),
                    HasLegacyMedia = !string.IsNullOrEmpty(lastMsg.MediaUrl)
                } : null,
                UnreadCount = unread
            };
        });

        await Send.OkAsync(new { Items = results, conversationsResult.HasMore }, ct);
    }
}
