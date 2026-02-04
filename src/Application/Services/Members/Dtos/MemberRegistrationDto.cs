namespace Application.Services.Members.Dtos;

public class MemberRegistrationDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int? PhoneExtension { get; set; }
    public DateTime BirthDate { get; set; }
    public int? Apartment { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Lang { get; set; } = null!;
}