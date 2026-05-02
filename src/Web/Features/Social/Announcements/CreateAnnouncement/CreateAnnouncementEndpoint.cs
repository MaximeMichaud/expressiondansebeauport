using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Application.Services.Push;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Announcements.CreateAnnouncement;

public class CreateAnnouncementRequest
{
    public string Content { get; set; } = null!;
    public List<CreateAnnouncementMediaItem> Media { get; set; } = new();
}

public class CreateAnnouncementMediaItem
{
    public string DisplayUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}

public class CreateAnnouncementEndpoint : Endpoint<CreateAnnouncementRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IPushNotificationDispatcher _dispatcher;

    public CreateAnnouncementEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IPushNotificationDispatcher dispatcher)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _dispatcher = dispatcher;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/announcements");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateAnnouncementRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.Media.Count > 1)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidAnnouncement", "Une annonce ne peut avoir qu'une seule image.")), ct);
            return;
        }

        if (req.Media.Any(m => !m.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)))
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidAnnouncement", "Les médias d'annonce doivent être des images.")), ct);
            return;
        }

        var media = req.Media.Select(m => new PostMediaItem
        {
            DisplayUrl = m.DisplayUrl,
            ThumbnailUrl = m.ThumbnailUrl,
            OriginalUrl = m.OriginalUrl,
            ContentType = m.ContentType,
            Size = m.Size
        }).ToList();

        var createdAnnouncement = await _postService.CreateAnnouncement(member.Id, req.Content, media);

        var allMembers = await _memberRepository.GetAll();
        var recipientUserIds = allMembers
            .Where(m => m.Id != member.Id)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        var preview = TruncateAnnouncementPreview(req.Content);
        await _dispatcher.SendToManyAsync(recipientUserIds, PushNotificationType.Announcement, new PushPayload
        {
            Title = "📢 Nouvelle annonce",
            Body = preview,
            Url = "/social/annonces",
            Tag = $"announcement-{createdAnnouncement.Id}"
        }, ct);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }

    private static string TruncateAnnouncementPreview(string content, int max = 120)
    {
        if (string.IsNullOrEmpty(content)) return "Nouvelle annonce";
        var stripped = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty).Trim();
        if (string.IsNullOrEmpty(stripped)) return "Nouvelle annonce";
        return stripped.Length <= max ? stripped : stripped.Substring(0, max - 1).TrimEnd() + "…";
    }
}
