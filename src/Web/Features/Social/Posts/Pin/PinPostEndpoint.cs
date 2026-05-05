using Application.Interfaces.Services.Users;
using Application.Services.Posts;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Posts.Pin;

public class PinPostEndpoint : Endpoint<PinPostRequest, PinPostResponse>
{
    private readonly IPostService _postService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;

    public PinPostEndpoint(
        IPostService postService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IGroupMemberRepository groupMemberRepository)
    {
        _postService = postService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _groupMemberRepository = groupMemberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/posts/{Id}/pin");
        Roles(Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(PinPostRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var isAdmin = User.IsInRole(Domain.Constants.User.Roles.ADMINISTRATOR);
        if (!isAdmin)
        {
            var isMember = await _groupMemberRepository.IsMember(req.GroupId, member.Id);
            if (!isMember)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
        }

        var result = await _postService.PinPost(req.Id, req.GroupId);
        await Send.OkAsync(new PinPostResponse(result.IsPinned, result.ReplacedExisting), ct);
    }
}
