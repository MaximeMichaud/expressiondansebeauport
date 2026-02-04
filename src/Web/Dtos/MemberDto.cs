namespace Web.Dtos;

public class MemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public int? PhoneExtension { get; set; }
    public int? Apartment { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public List<string> Roles { get; set; } = default!;
}