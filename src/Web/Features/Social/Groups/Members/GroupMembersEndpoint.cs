using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.Members;

public class GroupMembersEndpoint : Endpoint<GroupMembersRequest>
{
    private readonly IGroupMemberRepository _groupMemberRepository;

    public GroupMembersEndpoint(IGroupMemberRepository groupMemberRepository) =>
        _groupMemberRepository = groupMemberRepository;

    public override void Configure()
    {
        DontCatchExceptions();
        Get("social/groups/{GroupId}/members");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GroupMembersRequest req, CancellationToken ct)
    {
        const int pageSize = 50;
        var skip = (req.Page - 1) * pageSize;

        var members = await _groupMemberRepository.GetMembersOfGroup(req.GroupId, skip, pageSize);
        var totalCount = await _groupMemberRepository.GetMemberCount(req.GroupId);

        await Send.OkAsync(new
        {
            Items = members.Select(m => new
            {
                m.MemberId,
                m.Member.FirstName,
                m.Member.LastName,
                m.Member.FullName,
                m.Member.ProfileImageUrl,
                Role = m.Role.ToString(),
                m.JoinedAt
            }),
            Total = totalCount,
            req.Page,
            PageSize = pageSize
        }, ct);
    }
}
