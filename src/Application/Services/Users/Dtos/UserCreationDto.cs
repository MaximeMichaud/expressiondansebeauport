namespace Application.Services.Users.Dtos;

public class UserCreationDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public int? PhoneExtension { get; set; }
    public string RoleName { get; set; } = default!;
}