using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.GetFeed;

public class GetFeedEndpoint : Endpoint<GetFeedRequest>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IPostRepository _postRepository;

    public GetFeedEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IPostRepository postRepository)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _postRepository = postRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/{GroupId}/posts");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetFeedRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var posts = await _postService.GetGroupFeed(req.GroupId, req.Page);

        var result = posts.Select(p => new
        {
            p.Id,
            p.GroupId,
            AuthorMemberId = p.AuthorMemberId,
            AuthorName = p.AuthorMember?.FullName ?? "Inconnu",
            AuthorProfileImageUrl = p.AuthorMember?.ProfileImageUrl,
            p.Content,
            Type = p.Type.ToString(),
            p.IsPinned,
            p.ViewCount,
            LikeCount = p.Reactions?.Count ?? 0,
            CommentCount = p.Comments?.Count ?? 0,
            HasLiked = p.Reactions?.Any(r => r.MemberId == member.Id) ?? false,
            Media = p.Media?.Select(m => new
            {
                m.Id,
                m.MediaUrl,
                m.ThumbnailUrl,
                m.ContentType,
                m.Size,
                m.SortOrder
            }) ?? Enumerable.Empty<object>(),
            Poll = p.Poll != null ? new
            {
                p.Poll.Id,
                p.Poll.Question,
                p.Poll.AllowMultipleAnswers,
                HasVoted = p.Poll.Options.Any(o => o.Votes.Any(v => v.MemberId == member.Id)),
                Options = p.Poll.Options.Select(o => new
                {
                    o.Id,
                    o.Text,
                    VoteCount = o.Votes.Count,
                    Percentage = p.Poll.Options.Sum(x => x.Votes.Count) > 0
                        ? (int)Math.Round(o.Votes.Count * 100.0 / p.Poll.Options.Sum(x => x.Votes.Count))
                        : 0,
                    HasVoted = o.Votes.Any(v => v.MemberId == member.Id)
                })
            } : null,
            Created = p.Created.ToString()
        });

        await Send.OkAsync(result, ct);
    }
}
