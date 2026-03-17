using Domain.Common;
using Domain.Entities.Identity;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace Web.Features.Admins.Members.Promote;

public class PromoteProfessorEndpoint : Endpoint<PromoteProfessorRequest, SucceededOrNotResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly UserManager<User> _userManager;

    public PromoteProfessorEndpoint(IMemberRepository memberRepository, UserManager<User> userManager)
    {
        _memberRepository = memberRepository;
        _userManager = userManager;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/members/{Id}/promote");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(PromoteProfessorRequest req, CancellationToken ct)
    {
        var member = _memberRepository.FindById(req.Id);
        if (member == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(member.UserId.ToString());
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, Domain.Constants.User.Roles.PROFESSOR);
        if (!result.Succeeded)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false,
                result.Errors.Select(e => new Error(e.Code, e.Description))), ct);
            return;
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
