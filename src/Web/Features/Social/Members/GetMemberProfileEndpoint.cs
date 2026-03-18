using Application.Interfaces.Services.Users;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members;

public class GetMemberProfileRequest
{
    public Guid Id { get; set; }
}

public class GetMemberProfileEndpoint : Endpoint<GetMemberProfileRequest>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public GetMemberProfileEndpoint(
        IMemberRepository memberRepository,
        IGroupMemberRepository groupMemberRepository,
        IAuthenticatedUserService authenticatedUserService)
    {
        _memberRepository = memberRepository;
        _groupMemberRepository = groupMemberRepository;
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/members/{Id}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMemberProfileRequest req, CancellationToken ct)
    {
        var member = _memberRepository.FindById(req.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        var groups = await _groupMemberRepository.GetGroupsForMember(member.Id);

        await Send.OkAsync(new
        {
            member.Id,
            member.FullName,
            member.FirstName,
            member.LastName,
            member.Email,
            member.ProfileImageUrl,
            Roles = member.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Groups = groups.Select(g => new
            {
                g.Id,
                g.Name,
                g.Season,
                g.ImageUrl
            })
        }, ct);
    }
}
