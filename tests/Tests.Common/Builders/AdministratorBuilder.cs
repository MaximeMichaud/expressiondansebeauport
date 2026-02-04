using Domain.Entities;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using NodaTime;

namespace Tests.Common.Builders;

public class AdministratorBuilder
{
    private const string AnyFirstName = "john";
    private const string AnyLastName = "doe";
    private const string AnyEmail = "john.doe@gmail.com";
    private const string AnyPhoneNumber = "514-640-8920";
    private const int AnyPhoneExtension = 93;

    private Guid? Id { get; set; }
    private string? FirstName { get; set; }
    private string? LastName { get; set; }
    private string? Email { get; set; }
    private PhoneNumber? PhoneNumber { get; set; }
    private Instant? Deleted { get; set; }
    private string? DeletedBy { get; set; }
    private User? User { get; set; }
    private bool? Active { get; set; }

    public AdministratorBuilder WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public AdministratorBuilder WithFirstName(string firstName)
    {
        FirstName = firstName;
        return this;
    }

    public AdministratorBuilder WithLastName(string lastName)
    {
        LastName = lastName;
        return this;
    }

    public AdministratorBuilder WithEmail(string email)
    {
        Email = email;
        return this;
    }

    public AdministratorBuilder WithPhoneNumber(string phoneNumber, int? extension = null)
    {
        PhoneNumber = new PhoneNumber(phoneNumber, extension);
        return this;
    }

    public AdministratorBuilder WithUser(User user)
    {
        User = user;
        return this;
    }

    public AdministratorBuilder WithActive(bool active)
    {
        Active = active;
        return this;
    }

    public Administrator Build()
    {
        var admin = new Administrator(
            FirstName ?? AnyFirstName,
            LastName ?? AnyLastName
        );

        User ??= new User();
        User.PhoneNumber = PhoneNumber?.Number ?? AnyPhoneNumber;
        User.PhoneExtension = PhoneNumber?.Extension ?? AnyPhoneExtension;
        User.Email = Email ?? AnyEmail;
        admin.SetUser(User);

        if (DeletedBy != null || Deleted != null)
            admin.SoftDelete(DeletedBy);

        admin.SetId(Id ?? Guid.Empty);

        return admin;
    }
}