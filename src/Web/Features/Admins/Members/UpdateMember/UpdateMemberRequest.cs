using Web.Features.Common;

namespace Web.Features.Admins.Members.UpdateMember;

public class UpdateMemberRequest : ISanitizable
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public int? PhoneExtension { get; set; }
    public int? Apartment { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

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