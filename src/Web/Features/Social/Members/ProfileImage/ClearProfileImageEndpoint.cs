using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members.ProfileImage;

public class ClearProfileImageEndpoint : EndpointWithoutRequest<SucceededOrNotResponse>
{
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public ClearProfileImageEndpoint(
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("social/members/me/profile-image");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        var member = _memberRepository.FindByUserId(user.Id, asNoTracking: false);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        member.SetProfileImageUrl(null);
        await _memberRepository.Update(member);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
