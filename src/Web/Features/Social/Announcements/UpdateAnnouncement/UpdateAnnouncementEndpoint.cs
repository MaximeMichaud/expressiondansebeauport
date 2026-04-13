using Application.Services.Posts;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Announcements.UpdateAnnouncement;

public class UpdateAnnouncementRequest
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public List<UpdateAnnouncementMediaItem> Media { get; set; } = new();
}

public class UpdateAnnouncementMediaItem
{
    public string DisplayUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}

public class UpdateAnnouncementEndpoint : Endpoint<UpdateAnnouncementRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;

    public UpdateAnnouncementEndpoint(IPostService postService)
    {
        _postService = postService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/announcements/{Id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateAnnouncementRequest req, CancellationToken ct)
    {
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

        try
        {
            await _postService.UpdateAnnouncement(req.Id, req.Content, media);
            await Send.OkAsync(new SucceededOrNotResponse(true), ct);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidAnnouncement", ex.Message)), ct);
        }
    }
}
