using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.Comments;

public class GetCommentsEndpoint : Endpoint<GetCommentsRequest>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetCommentsEndpoint(
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
        Get("social/posts/{PostId}/comments");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetCommentsRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.OkAsync(new List<object>(), ct);
            return;
        }

        var comments = await _postService.GetComments(req.PostId, req.Page);

        var result = comments.Select(c => new
        {
            c.Id,
            c.PostId,
            AuthorMemberId = c.AuthorMemberId,
            AuthorName = c.AuthorMember?.FullName ?? "Inconnu",
            AuthorProfileImageUrl = c.AuthorMember?.ProfileImageUrl,
            AuthorAvatarColor = c.AuthorMember?.AvatarColor ?? "#1a1a1a",
            AuthorRoles = c.AuthorMember?.User?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string?>(),
            c.Content,
            Created = c.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        await Send.OkAsync(result, ct);
    }
}
