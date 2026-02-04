using Web.Features.Common;

namespace Web.Features.Public.Authentication.TwoFactor;

public class TwoFactorRequest : ISanitizable
{
    public string Username { get; set; } = null!;
    public string Code { get; set; } = null!;

    public void Sanitize()
    {
        Username = Username.Trim();
        Code = Code.Trim();
    }
}