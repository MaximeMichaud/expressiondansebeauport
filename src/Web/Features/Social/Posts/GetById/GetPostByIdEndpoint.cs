using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.GetById;

public class GetPostByIdEndpoint : Endpoint<GetPostByIdRequest>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetPostByIdEndpoint(
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
        Get("social/posts/{Id}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetPostByIdRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var post = await _postService.GetPostById(req.Id);
        if (post == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new
        {
            post.Id,
            AuthorName = post.AuthorMember?.FullName ?? "Inconnu",
            AuthorRoles = post.AuthorMember?.User?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string?>(),
            post.Content,
            Type = post.Type.ToString(),
            post.IsPinned,
            post.ViewCount,
            LikeCount = post.Reactions?.Count ?? 0,
            CommentCount = post.Comments?.Count ?? 0,
            HasLiked = post.Reactions?.Any(r => r.MemberId == member.Id) ?? false,
            Created = post.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ")
        }, ct);
    }
}
