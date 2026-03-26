using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.Comments;

public class DeleteCommentEndpoint : Endpoint<DeleteCommentRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        ICommentRepository commentRepository)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _commentRepository = commentRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("social/comments/{Id}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteCommentRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var comment = await _commentRepository.FindById(req.Id);
        if (comment == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var isAdmin = user.HasRole(Domain.Constants.User.Roles.ADMINISTRATOR);
        if (comment.AuthorMemberId != member.Id && !isAdmin)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await _postService.DeleteComment(req.Id);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
