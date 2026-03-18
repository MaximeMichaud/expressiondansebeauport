using Application.Interfaces.Services.Users;
using Application.Services.Messaging;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Messages.UnreadCount;

public class UnreadCountEndpoint : EndpointWithoutRequest
{
    private readonly IConversationService _conversationService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public UnreadCountEndpoint(
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
        Get("social/messages/unread-count");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Count = 0 }, ct);
            return;
        }

        var count = await _conversationService.GetUnreadCount(member.Id);
        await HttpContext.Response.WriteAsJsonAsync(new { Count = count }, ct);
    }
}
