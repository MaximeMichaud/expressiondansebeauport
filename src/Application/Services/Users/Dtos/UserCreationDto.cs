namespace Application.Services.Users.Dtos;

public class UserCreationDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int? PhoneExtension { get; set; }
    public string RoleName { get; set; } = null!;
}