using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Announcements.GetAnnouncements;

public class GetAnnouncementsEndpoint : Endpoint<GetAnnouncementsRequest>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public GetAnnouncementsEndpoint(
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
        Get("social/announcements");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAnnouncementsRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.OkAsync(new List<object>(), ct);
            return;
        }

        var announcements = await _postService.GetAnnouncements(req.Page);

        var result = announcements.Select(p => new
        {
            p.Id,
            AuthorName = p.AuthorMember?.FullName ?? "Inconnu",
            AuthorRoles = p.AuthorMember?.User?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string?>(),
            p.Content,
            Type = p.Type.ToString(),
            p.ViewCount,
            LikeCount = p.Reactions?.Count ?? 0,
            CommentCount = p.Comments?.Count ?? 0,
            HasLiked = p.Reactions?.Any(r => r.MemberId == member.Id) ?? false,
            Media = p.Media?.Select(m => new
            {
                m.Id,
                m.MediaUrl,
                m.ThumbnailUrl,
                m.ContentType
            }) ?? Enumerable.Empty<object>(),
            Created = p.Created.ToDateTimeUtc().ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        await Send.OkAsync(result, ct);
    }
}
