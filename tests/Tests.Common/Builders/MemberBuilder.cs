using Domain.Entities;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using NodaTime;

namespace Tests.Common.Builders;

public class MemberBuilder
{
    private const string AnyFirstName = "john";
    private const string AnyLastName = "doe";
    private const string AnyEmail = "john.doe@gmail.com";
    private const string AnyPhoneNumber = "514-640-8920";
    private const int AnyPhoneExtension = 93;
    private const string AnyStreet = "965, Hollywood Blvd";
    private const string AnyCity = "Hollywood";
    private const string AnyZipCode = "G7E 3L8";

    private Guid? Id { get; set; }
    private string? FirstName { get; set; }
    private string? LastName { get; set; }
    private string? Email { get; set; }
    private PhoneNumber? PhoneNumber { get; set; }
    private int? Apartment { get; set; }
    private string? Street { get; set; }
    private string? City { get; set; }
    private string? ZipCode { get; set; }
    private Instant? Deleted { get; set; }
    private string? DeletedBy { get; set; }
    private User? User { get; set; }
    private bool? Active { get; set; }

    public MemberBuilder WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public MemberBuilder WithFirstName(string firstName)
    {
        FirstName = firstName;
        return this;
    }

    public MemberBuilder WithLastName(string lastName)
    {
        LastName = lastName;
        return this;
    }

    public MemberBuilder WithEmail(string email)
    {
        Email = email;
        return this;
    }

    public MemberBuilder WithPhoneNumber(string phoneNumber, int? extension = null)
    {
        PhoneNumber = new PhoneNumber(phoneNumber, extension);
        return this;
    }

    public MemberBuilder WithApartment(int apartment)
    {
        Apartment = apartment;
        return this;
    }

    public MemberBuilder WithStreet(string street)
    {
        Street = street;
        return this;
    }

    public MemberBuilder WithCity(string city)
    {
        City = city;
        return this;
    }

    public MemberBuilder WithZipCode(string zipCode)
    {
        ZipCode = zipCode;
        return this;
    }

    public MemberBuilder WithUser(User user)
    {
        User = user;
        return this;
    }

    public MemberBuilder WithActive(bool active)
    {
        Active = active;
        return this;
    }

    public Member Build()
    {
        var member = new Member(
            FirstName ?? AnyFirstName,
            LastName ?? AnyLastName,
            Apartment,
            Street ?? AnyStreet,
            City ?? AnyCity,
            ZipCode ?? AnyZipCode
        );

        User ??= new User();
        User.PhoneNumber = PhoneNumber?.Number ?? AnyPhoneNumber;
        User.PhoneExtension = PhoneNumber?.Extension ?? AnyPhoneExtension;
        User.Email = Email ?? AnyEmail;
        member.SetUser(User);

        if (Active.HasValue && Active.Value)
            member.Activate();

        if (Active.HasValue && Active.Value)
            member.Activate();

        if (DeletedBy != null || Deleted != null)
            member.SoftDelete(DeletedBy);

        member.SetId(Id ?? Guid.Empty);

        return member;
    }
}