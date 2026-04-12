using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Admin;

public class GetAdminMessagesEndpoint : Endpoint<GetAdminMessagesRequest>
{
    private readonly IConversationService _conversationService;

    public GetAdminMessagesEndpoint(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/admin/conversations/{ConversationId}/messages");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAdminMessagesRequest req, CancellationToken ct)
    {
        var messagesResult = await _conversationService.GetMessages(req.ConversationId, req.Page);
        var messages = messagesResult.Items;

        var results = messages.Select(m =>
        {
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
