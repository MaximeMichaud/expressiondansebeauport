using Application.Interfaces.Services.Users;
using Application.Services.Posts;
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

    public CreatePostEndpoint(
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
            await _postService.CreatePost(req.GroupId, member.Id, req.Content, type, media);
        }
        catch (InvalidOperationException ex)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidPost", ex.Message)), ct);
            return;
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
