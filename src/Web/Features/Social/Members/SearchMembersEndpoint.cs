using Application.Interfaces.Services.Users;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members;

public class SearchMembersEndpoint : Endpoint<SearchMembersRequest>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public SearchMembersEndpoint(
        IMemberRepository memberRepository,
        IAuthenticatedUserService authenticatedUserService)
    {
        _memberRepository = memberRepository;
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/members/search");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(SearchMembersRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        var members = await _memberRepository.Search(req.Query, 0, 500);
        var currentMember = _memberRepository.FindByUserId(user.Id);

        var results = members
            .Where(m => currentMember == null || m.Id != currentMember.Id)
            .Select(m => new
            {
                m.Id,
                m.FullName,
                m.Email,
                m.ProfileImageUrl, m.AvatarColor,
                Roles = m.User.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .OrderBy(m => m.FullName)
            .ToList();

        await Send.OkAsync(results, ct);
    }
}
