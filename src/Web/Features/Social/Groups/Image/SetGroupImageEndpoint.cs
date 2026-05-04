using Application.Interfaces.Services.Users;
using Application.Services.Groups;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Groups.Image;

public class SetGroupImageEndpoint : Endpoint<SetGroupImageRequest, SucceededOrNotResponse>
{
    private readonly IGroupService _groupService;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public SetGroupImageEndpoint(
        IGroupService groupService,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _groupService = groupService;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/groups/{GroupId}/image");
        Roles(Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(SetGroupImageRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        var member = _memberRepository.FindByUserId(user.Id);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        try
        {
            await _groupService.SetImageForMember(req.GroupId, member.Id, req.ImageUrl);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Not a member"))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
