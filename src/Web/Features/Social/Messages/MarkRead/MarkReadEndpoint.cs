using Application.Interfaces.Services.Users;
using Application.Services.Messaging;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Messages.MarkRead;

public class MarkReadEndpoint : EndpointWithoutRequest<SucceededOrNotResponse>
{
    private readonly IConversationService _conversationService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public MarkReadEndpoint(
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
        Put("social/conversations/{conversationId}/read");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var conversationId = Route<Guid>("conversationId");

        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _conversationService.MarkAsRead(conversationId, member.Id);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
