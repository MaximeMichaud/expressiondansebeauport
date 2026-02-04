namespace Web.Features.Public.Authentication.ResetPassword;

public class ResetPasswordRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirmation { get; set; } = null!;
}