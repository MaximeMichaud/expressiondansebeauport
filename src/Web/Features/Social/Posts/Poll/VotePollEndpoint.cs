using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.Poll;

public class VotePollEndpoint : Endpoint<VotePollRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public VotePollEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/posts/{PostId}/poll/vote");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(VotePollRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _postService.VoteOnPoll(req.PollOptionId, member.Id);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
