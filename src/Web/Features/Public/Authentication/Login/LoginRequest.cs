using Web.Features.Common;

namespace Web.Features.Public.Authentication.Login;

public class LoginRequest : ISanitizable
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

    public void Sanitize()
    {
        Username = Username.Trim();
    }
}