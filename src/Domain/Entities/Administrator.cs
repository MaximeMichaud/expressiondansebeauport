using Domain.Common;
using Domain.Entities.Identity;
using Domain.Extensions;

namespace Domain.Entities;

public class Administrator : AuditableAndSoftDeletableEntity, ISanitizable
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string Email => User.Email!;
    public User User { get; private set; } = null!;

    public Administrator() {}

    public Administrator(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void SetFirstName(string firstName) => FirstName = firstName;
    public void SetLastName(string lastName) => LastName = lastName;
    public void SetUser(User user) => User = user;

    public override void SoftDelete(string? deletedBy = null)
    {
        base.SoftDelete(deletedBy);
        User.SoftDelete(deletedBy);
    }

    public void SanitizeForSaving()
    {
        FirstName = FirstName.Trim().CapitalizeFirstLetterOfEachWord()!;
        LastName = LastName.Trim().CapitalizeFirstLetterOfEachWord()!;
    }
}