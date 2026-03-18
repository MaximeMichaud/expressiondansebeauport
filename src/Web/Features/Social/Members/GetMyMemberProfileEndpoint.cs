using Application.Interfaces.Services.Users;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members;

public class GetMyMemberProfileEndpoint : EndpointWithoutRequest
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public GetMyMemberProfileEndpoint(
        IMemberRepository memberRepository,
        IAuthenticatedUserService authenticatedUserService)
    {
        _memberRepository = memberRepository;
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/members/me");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var member = _memberRepository.FindByUserId(user!.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        await Send.OkAsync(new
        {
            member.Id,
            member.FullName,
            member.FirstName,
            member.LastName,
            member.Email,
            member.ProfileImageUrl,
        }, ct);
    }
}
