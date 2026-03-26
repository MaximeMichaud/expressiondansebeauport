using Application.Services.Members;
using Domain.Common;
using Domain.Entities.Identity;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Web.Features.Public.Members.Confirm;

public class ConfirmEndpoint : Endpoint<ConfirmRequest, SucceededOrNotResponse>
{
    private readonly IEmailConfirmationService _confirmationService;
    private readonly UserManager<User> _userManager;

    public ConfirmEndpoint(
        IEmailConfirmationService confirmationService,
        UserManager<User> userManager)
    {
        _confirmationService = confirmationService;
        _userManager = userManager;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("members/confirm");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ConfirmRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email.ToLowerInvariant());
        if (user == null)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false), ct);
            return;
        }

        var confirmed = await _confirmationService.ConfirmEmail(user.Id, req.Code);
        await Send.OkAsync(new SucceededOrNotResponse(confirmed), ct);
    }
}
