using Web.Features.Common;

namespace Web.Features.Public.Members.Register;

public class RegisterRequest : ISanitizable
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public void Sanitize()
    {
        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        Email = Email.Trim().ToLowerInvariant();
    }
}
