using Domain.Entities.Identity;
using FluentEmail.Core;
using NodaTime;

namespace Tests.Common.Builders;

public class UserBuilder
{
    private const string ANY_EMAIL = "john.doe@gmail.com";

    private Guid? Id { get; set; }
    private string? Email { get; set; }
    private string? PhoneNumber { get; set; }
    private Instant? Deleted { get; set; }
    private string? DeletedBy { get; set; }
    private IList<Role> Roles { get; set; } = new List<Role>();

    public UserBuilder WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public UserBuilder WithEmail(string? email)
    {
        Email = email;
        return this;
    }

    public UserBuilder WithPhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
        return this;
    }

    public UserBuilder WithRole(Role role)
    {
        Roles.Add(role);
        return this;
    }

    public UserBuilder AsNotDeleted()
    {
        Deleted = null;
        DeletedBy = null;
        return this;
    }

    public UserBuilder AsDeleted()
    {
        Deleted = Instant.MaxValue;
        DeletedBy = ANY_EMAIL;
        return this;
    }

    public User Build()
    {
        var user = new User
        {
            Id = Id ?? Guid.Empty,
            SecurityStamp = null,
            EmailConfirmed = true,
            PhoneNumber = PhoneNumber,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = true,
        };

        if (!string.IsNullOrWhiteSpace(Email))
        {
            user.Email = Email.ToLower();
            user.UserName = Email.ToLower();
            user.NormalizedEmail = Email.ToUpperInvariant();
            user.NormalizedUserName = Email.ToUpperInvariant();
        }

        if (Roles.Any())
            Roles.ForEach(role => user.AddRole(role));

        if (DeletedBy != null || Deleted != null)
            user.SoftDelete(DeletedBy);

        return user;
    }
}