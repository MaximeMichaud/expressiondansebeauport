namespace Web.Features.Users.Me.ChangePassword;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string NewPasswordConfirmation { get; set; } = null!;
}
