using Web.Features.Common;

namespace Web.Features.Public.Authentication.Login;

public class LoginRequest : ISanitizable
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public void Sanitize()
    {
        Username = Username.Trim();
    }
}