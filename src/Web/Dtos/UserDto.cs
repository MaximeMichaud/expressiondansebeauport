namespace Web.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int? PhoneExtension { get; set; }
    public List<string> Roles { get; set; } = null!;
}