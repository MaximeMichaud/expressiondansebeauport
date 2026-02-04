using Web.Features.Common;

namespace Web.Features.Public.Authentication.ForgotPassword;

public class ForgotPasswordRequest : ISanitizable
{
    public string Username { get; set; } = null!;
    public string ResetPasswordRelativeUrl { get; set; } = null!;

    public void Sanitize()
    {
        Username = Username.Trim();
        ResetPasswordRelativeUrl = ResetPasswordRelativeUrl.Trim();
    }
}