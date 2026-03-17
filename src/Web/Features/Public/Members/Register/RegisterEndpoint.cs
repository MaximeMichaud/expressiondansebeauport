using Application.Interfaces.Services.Notifications;
using Application.Services.Members;
using Domain.Common;
using Web.Features.Common;

namespace Web.Features.Public.Members.Register;

public class RegisterEndpoint : EndpointWithSanitizedRequest<RegisterRequest, SucceededOrNotResponse>
{
    private readonly IMemberRegistrationService _registrationService;
    private readonly INotificationService _notificationService;

    public RegisterEndpoint(
        IMemberRegistrationService registrationService,
        INotificationService notificationService)
    {
        _registrationService = registrationService;
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("members/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var (member, code) = await _registrationService.RegisterMember(
            req.FirstName, req.LastName, req.Email, req.Password);

        await _notificationService.SendConfirmationCodeNotification(member.User, code);

        await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
    }
}
