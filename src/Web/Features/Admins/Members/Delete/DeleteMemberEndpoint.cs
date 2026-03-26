using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Members.Delete;

public class DeleteMemberRequest
{
    public Guid Id { get; set; }
}

public class DeleteMemberEndpoint : Endpoint<DeleteMemberRequest, SucceededOrNotResponse>
{
    private readonly IMemberRepository _memberRepository;

    public DeleteMemberEndpoint(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/members/{Id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMemberRequest req, CancellationToken ct)
    {
        var member = _memberRepository.FindById(req.Id, asNoTracking: false);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        member.SoftDelete();
        await _memberRepository.Update(member);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
