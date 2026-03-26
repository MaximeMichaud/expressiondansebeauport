namespace Web.Features.Public.Members.Confirm;

public class ConfirmRequest
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
