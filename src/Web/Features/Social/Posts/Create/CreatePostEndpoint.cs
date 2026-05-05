using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Application.Services.Push;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.Create;

public class CreatePostEndpoint : Endpoint<CreatePostRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IPushNotificationDispatcher _dispatcher;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly IGroupRepository _groupRepo;

    public CreatePostEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IPushNotificationDispatcher dispatcher,
        IGroupMemberRepository groupMemberRepo,
        IGroupRepository groupRepo)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _dispatcher = dispatcher;
        _groupMemberRepo = groupMemberRepo;
        _groupRepo = groupRepo;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/posts");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreatePostRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var type = Enum.Parse<Domain.Enums.PostType>(req.Type, true);

        if (type == Domain.Enums.PostType.Photo && req.Media.Count == 0)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidPost", "Une publication photo doit inclure au moins une image.")), ct);
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

        Domain.Entities.Post createdPost;
        try
        {
            createdPost = await _postService.CreatePost(req.GroupId, member.Id, req.Content, type, media);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidPost", ex.Message)), ct);
            return;
        }

        if (req.GroupId != Guid.Empty)
        {
            var group = await _groupRepo.FindById(req.GroupId);
            var groupMembers = await _groupMemberRepo.GetMembersOfGroup(req.GroupId, skip: 0, take: int.MaxValue);
            var recipientUserIds = groupMembers
                .Where(gm => gm.MemberId != member.Id)
                .Select(gm => gm.Member.UserId)
                .Distinct()
                .ToList();

            var preview = TruncatePostPreview(req.Content);
            await _dispatcher.SendToManyAsync(recipientUserIds, PushNotificationType.GroupPost, new PushPayload
            {
                Title = $"{group?.Name ?? "Nouveau post"} · {member.FullName}",
                Body = preview,
                Url = $"/social/posts/{createdPost.Id}",
                Tag = $"post-{createdPost.Id}",
                GroupId = req.GroupId
            }, ct);
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }

    private static string TruncatePostPreview(string content, int max = 120)
    {
        if (string.IsNullOrEmpty(content)) return "Nouvelle publication";
        var stripped = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty).Trim();
        if (string.IsNullOrEmpty(stripped)) return "Nouvelle publication";
        return stripped.Length <= max ? stripped : stripped.Substring(0, max - 1).TrimEnd() + "…";
    }
}
