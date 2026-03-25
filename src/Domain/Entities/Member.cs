using Domain.Common;
using Domain.Entities.Identity;
using Domain.Extensions;

namespace Domain.Entities;

public class Member : AuditableAndSoftDeletableEntity, ISanitizable
{
    private static readonly string[] AvatarColors = [
        "#e53e3e", "#dd6b20", "#d69e2e", "#38a169", "#319795",
        "#3182ce", "#5a67d8", "#805ad5", "#d53f8c", "#e53e3e"
    ];

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? ProfileImageUrl { get; private set; }
    public string AvatarColor { get; private set; } = "#1a1a1a";
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string FullName => $"{FirstName} {LastName}";
    public string Email => User?.Email ?? string.Empty;

    public Member() {}

    public Member(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        AvatarColor = AvatarColors[Random.Shared.Next(AvatarColors.Length)];
    }

    public void SetFirstName(string firstName) => FirstName = firstName;
    public void SetLastName(string lastName) => LastName = lastName;
    public void SetProfileImageUrl(string? url) => ProfileImageUrl = url;
    public void SetAvatarColor(string color) => AvatarColor = color;
    public void AssignRandomAvatarColor() => AvatarColor = AvatarColors[Random.Shared.Next(AvatarColors.Length)];
    public void SetUser(User user)
    {
        User = user;
        UserId = user.Id;
    }

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
