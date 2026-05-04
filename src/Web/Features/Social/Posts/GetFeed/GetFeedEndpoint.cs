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
    private readonly IGroupMemberRepository _groupMemberRepository;

    public GetFeedEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IPostRepository postRepository,
        IGroupMemberRepository groupMemberRepository)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _postRepository = postRepository;
        _groupMemberRepository = groupMemberRepository;
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

        var feedResult = await _postService.GetGroupFeed(req.GroupId, req.Page);
        var posts = feedResult.Items;

        // Build role lookup for authors in this group
        var authorIds = posts.Select(p => p.AuthorMemberId).Where(id => id != Guid.Empty).Distinct().ToList();
        var groupMemberRoles = req.GroupId != Guid.Empty
            ? await _groupMemberRepository.GetRolesForMembers(req.GroupId, authorIds)
            : new Dictionary<Guid, string>();

        // Get comment counts individually (reliable, same approach as GetPostById)
        var commentCounts = new Dictionary<Guid, int>();
        foreach (var p in posts)
            commentCounts[p.Id] = await _postRepository.GetCommentCount(p.Id);

        var result = posts.Select(p => new
        {
            p.Id,
            p.GroupId,
            AuthorMemberId = p.AuthorMemberId,
            AuthorName = p.AuthorMember?.FullName ?? "Inconnu",
            AuthorProfileImageUrl = p.AuthorMember?.ProfileImageUrl,
            AuthorAvatarColor = p.AuthorMember?.AvatarColor ?? "#1a1a1a",
            AuthorRole = groupMemberRoles.TryGetValue(p.AuthorMemberId, out var role) ? role : "Member",
            AuthorRoles = p.AuthorMember?.User?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string?>(),
            p.Content,
            Type = p.Type.ToString(),
            p.IsPinned,
            p.ViewCount,
            LikeCount = p.Reactions?.Count ?? 0,
            CommentCount = commentCounts.TryGetValue(p.Id, out var cc) ? cc : 0,
            HasLiked = p.Reactions?.Any(r => r.MemberId == member.Id) ?? false,
            Media = p.Media?.Select(m => new
            {
                m.Id,
                m.MediaUrl,
                m.ThumbnailUrl,
                m.OriginalUrl,
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
            Created = p.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        await Send.OkAsync(new { Items = result, feedResult.HasMore }, ct);
    }
}
