using Application.Exceptions.Members;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Notifications;
using Application.Services.Members;
using Domain.Common;
using Web.Features.Common;

namespace Web.Features.Public.Members.Register;

public class RegisterEndpoint : EndpointWithSanitizedRequest<RegisterRequest, SucceededOrNotResponse>
{
    private readonly IMemberRegistrationService _registrationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RegisterEndpoint> _logger;

    public RegisterEndpoint(
        IMemberRegistrationService registrationService,
        INotificationService notificationService,
        ILogger<RegisterEndpoint> logger)
    {
        _registrationService = registrationService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("members/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var (member, code) = await _registrationService.RegisterMember(
                req.FirstName, req.LastName, req.Email, req.Password);

            _logger.LogInformation("Confirmation code for {Email}: {Code}", req.Email, code);

            try
            {
                await _notificationService.SendConfirmationCodeNotification(member.User, code);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send confirmation email to {Email}. Code: {Code}", req.Email, code);
            }

            await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
        }
        catch (UserWithEmailAlreadyExistsException)
        {
            var error = new Error("EmailExists", "Un compte avec ce courriel existe déjà.");
            await Send.OkAsync(new SucceededOrNotResponse(false, error), ct);
        }
        catch (UserCreationException ex)
        {
            var error = new Error("ValidationError", ex.Message);
            await Send.OkAsync(new SucceededOrNotResponse(false, error), ct);
        }
    }
}
