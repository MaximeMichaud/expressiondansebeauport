using Application.Interfaces.Services.Users;
using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Messages.GetConversations;

public class GetConversationsEndpoint : Endpoint<GetConversationsRequest>
{
    private readonly IConversationService _conversationService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetConversationsEndpoint(
        IConversationService conversationService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _conversationService = conversationService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/conversations");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetConversationsRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.OkAsync(new List<object>(), ct);
            return;
        }

        var conversations = await _conversationService.GetConversations(member.Id, req.Page);

        var results = conversations.Select(c =>
        {
            var otherMemberId = c.ParticipantAMemberId == member.Id
                ? c.ParticipantBMemberId
                : c.ParticipantAMemberId;
            var other = c.ParticipantAMemberId == member.Id ? c.ParticipantB : c.ParticipantA;
            var lastMsg = c.Messages?.OrderByDescending(m => m.Created).FirstOrDefault();
            var participant = c.Participants?.FirstOrDefault(p => p.MemberId == member.Id);
            // Check if last message is from the other person and unread
            var unread = 0;
            if (lastMsg != null && lastMsg.SenderMemberId != member.Id)
            {
                if (participant?.LastReadAt == null || lastMsg.Created > participant.LastReadAt)
                    unread = 1;
            }

            return new
            {
                c.Id,
                OtherMember = new
                {
                    Id = otherMemberId,
                    FullName = other?.FullName ?? "Inconnu",
                    ProfileImageUrl = other?.ProfileImageUrl,
                    AvatarColor = other?.AvatarColor ?? "#1a1a1a",
                    Roles = other?.User?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string?>()
                },
                LastMessage = lastMsg != null ? new
                {
                    Content = lastMsg.Content,
                    SenderName = lastMsg.SenderMember?.FullName ?? "",
                    IsMine = lastMsg.SenderMemberId == member.Id,
                    Created = lastMsg.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    MediaCount = lastMsg.Media?.Count ?? 0,
                    HasVideo = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("video/")),
                    HasImage = lastMsg.Media != null && lastMsg.Media.Any(m => m.ContentType != null && m.ContentType.StartsWith("image/")),
                    HasLegacyMedia = !string.IsNullOrEmpty(lastMsg.MediaUrl)
                } : null,
                UnreadCount = unread
            };
        });

        await Send.OkAsync(results, ct);
    }
}
