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

        var messages = await _conversationService.GetMessages(req.ConversationId, req.Page);

        // Get the other participant's LastReadAt to determine read receipts
        var conversation = await _conversationRepository.FindById(req.ConversationId);
        var otherParticipant = conversation?.Participants?.FirstOrDefault(p => p.MemberId != member.Id);
        var otherLastReadAt = otherParticipant?.LastReadAt;

        var results = messages.Select(m => new
        {
            m.Id,
            m.ConversationId,
            SenderMemberId = m.SenderMemberId,
            SenderName = m.SenderMember?.FullName ?? "Inconnu",
            Content = m.Deleted.HasValue ? null : m.Content,
            Created = m.Created.ToString(),
            IsDeleted = m.Deleted.HasValue,
            IsRead = m.SenderMemberId == member.Id && otherLastReadAt != null && m.Created <= otherLastReadAt
        });

        await Send.OkAsync(results, ct);
    }
}
