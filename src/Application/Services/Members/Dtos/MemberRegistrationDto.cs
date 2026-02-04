namespace Application.Services.Members.Dtos;

public class MemberRegistrationDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public int? PhoneExtension { get; set; }
    public DateTime BirthDate { get; set; }
    public int? Apartment { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Lang { get; set; } = default!;
}