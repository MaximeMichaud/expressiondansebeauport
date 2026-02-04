namespace Web.Features.Public.Authentication.ResetPassword;

public class ResetPasswordRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PasswordConfirmation { get; set; } = default!;
}