using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Members.GetAll;

public class GetAllMembersEndpoint : Endpoint<GetAllMembersRequest>
{
    private readonly IMemberRepository _memberRepository;

    public GetAllMembersEndpoint(IMemberRepository memberRepository) => _memberRepository = memberRepository;

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/members");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAllMembersRequest req, CancellationToken ct)
    {
        var skip = (req.Page - 1) * req.PageSize;
        var members = await _memberRepository.Search(req.Search, skip, req.PageSize);
        var total = await _memberRepository.Count(req.Search);

        await Send.OkAsync(new
        {
            Items = members.Select(m => new
            {
                m.Id,
                m.FirstName,
                m.LastName,
                m.FullName,
                m.Email,
                m.ProfileImageUrl, m.AvatarColor,
                m.UserId,
                Roles = m.User.RoleNames
            }),
            Total = total,
            req.Page,
            req.PageSize
        }, ct);
    }
}
