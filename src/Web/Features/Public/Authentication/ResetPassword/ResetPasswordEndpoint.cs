using Application.Extensions;
using Domain.Common;
using Domain.Extensions;
using Domain.Repositories;
using FastEndpoints;

namespace Web.Features.Public.Authentication.ResetPassword;

public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest, SucceededOrNotResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ResetPasswordEndpoint> _logger;

    public ResetPasswordEndpoint(
        IUserRepository userRepository,
        ILogger<ResetPasswordEndpoint> logger)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("authentication/reset-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var user = _userRepository.FindById(req.UserId);
        if (user == null)
        {
            _logger.LogInformation("Could not reset password since no user with user id {id} exists.", req.UserId);
            await Send.OkAsync(new SucceededOrNotResponse(false), ct);
            return;
        }

        var identityResult = await _userRepository.ResetUserPassword(user, req.Password, req.Token.Base64UrlDecode());
        if (!identityResult.Succeeded)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, identityResult.GetErrors()), ct);
            return;
        }

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}