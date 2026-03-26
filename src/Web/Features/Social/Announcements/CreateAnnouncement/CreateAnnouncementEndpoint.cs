using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Announcements.CreateAnnouncement;

public class CreateAnnouncementRequest
{
    public string Content { get; set; } = null!;
}

public class CreateAnnouncementEndpoint : Endpoint<CreateAnnouncementRequest, SucceededOrNotResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public CreateAnnouncementEndpoint(
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

        await _postService.CreateAnnouncement(member.Id, req.Content);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
