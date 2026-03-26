using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Messages.DeleteMessage;

public class DeleteMessageRequest
{
    public Guid MessageId { get; set; }
}

public class DeleteMessageEndpoint : Endpoint<DeleteMessageRequest, SucceededOrNotResponse>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public DeleteMessageEndpoint(
        IMessageRepository messageRepository,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _messageRepository = messageRepository;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("social/messages/{MessageId}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMessageRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        var message = await _messageRepository.FindById(req.MessageId);
        if (message == null) { await Send.NotFoundAsync(ct); return; }

        // Only the sender can delete their own message
        if (message.SenderMemberId != member.Id)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        message.SoftDelete();
        await _messageRepository.SaveChanges();

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
