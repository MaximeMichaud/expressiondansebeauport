using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members.DeleteMe;

public class DeleteMeEndpoint : EndpointWithoutRequest<SucceededOrNotResponse>
{
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public DeleteMeEndpoint(
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("social/members/me");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        var member = _memberRepository.FindById(user.Id, asNoTracking: false)
            ?? _memberRepository.FindByUserId(user.Id, asNoTracking: false);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        member.SoftDelete();
        await _memberRepository.Update(member);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
