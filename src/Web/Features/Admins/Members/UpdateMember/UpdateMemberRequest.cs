using Web.Features.Common;

namespace Web.Features.Admins.Members.UpdateMember;

public class UpdateMemberRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int? PhoneExtension { get; set; }
    public int? Apartment { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;

    public void Sanitize()
    {
        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        Email = Email.Trim();
        PhoneNumber = PhoneNumber.Trim();
        Street = Street.Trim();
        City = City.Trim();
        ZipCode = ZipCode.Trim();
    }
}