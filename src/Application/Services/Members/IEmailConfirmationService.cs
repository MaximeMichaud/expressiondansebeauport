namespace Application.Services.Members;

public interface IEmailConfirmationService
{
    Task<bool> ConfirmEmail(Guid userId, string code);
    Task<string> ResendCode(Guid userId);
}
