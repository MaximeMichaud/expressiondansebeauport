using Domain.Common;
using Domain.Entities.Identity;
using Domain.Extensions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Member : AuditableAndSoftDeletableEntity, ISanitizable
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string Email => User.Email!;
    public PhoneNumber? PhoneNumber => string.IsNullOrWhiteSpace(User.PhoneNumber)
        ? null : new PhoneNumber(User.PhoneNumber, User.PhoneExtension);
    public int? Apartment { get; private set; }
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? ZipCode { get; private set; }
    public User User { get; private set; } = null!;

    public bool Active { get; private set; }

    public Member() {}

    public Member(string firstName, string lastName, int? apartment = null,
        string? street = null, string? city = null, string? zipCode = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Apartment = apartment;
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    public string? GetPhoneNumber() => PhoneNumber?.Number ?? null;
    public int? GetPhoneExtension() => PhoneNumber?.Extension ?? null;

    public void SetFirstName(string firstName) => FirstName = firstName;
    public void SetLastName(string lastName) => LastName = lastName;
    public void SetApartment(int? apartment) => Apartment = apartment;
    public void SetStreet(string? street) => Street = street;
    public void SetCity(string? city) => City = city;
    public void SetZipCode(string? zipCode) => ZipCode = zipCode;
    public void SetUser(User user) => User = user;

    public void OnCreated(User user)
    {
        SetUser(user);
    }

    public void Activate()
    {
        Restore();
        Active = true;
        User.Activate(FirstName);
    }

    public void Deactivate(string? deletedBy = null)
    {
        Active = false;
        User.SoftDelete(deletedBy);
    }

    public override void SoftDelete(string? deletedBy = null)
    {
        base.SoftDelete(deletedBy);
        Active = false;
        User.SoftDelete(deletedBy);
    }

    public void SanitizeForSaving()
    {
        FirstName = FirstName.Trim().CapitalizeFirstLetterOfEachWord()!;
        LastName = LastName.Trim().CapitalizeFirstLetterOfEachWord()!;
        Street = Street?.Trim().CapitalizeFirstLetterOfEachWord();
        City = City?.Trim().CapitalizeFirstLetterOfEachWord();
        ZipCode = ZipCode?.Trim().ToUpper();
    }
}