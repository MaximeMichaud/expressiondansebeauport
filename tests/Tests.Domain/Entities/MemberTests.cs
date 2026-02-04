using Domain.Entities;
using NodaTime;
using Tests.Common.Builders;

namespace Tests.Domain.Entities;

public class MemberTests
{
    private const string AnyEmail = "garneau@spektrummedia.com";

    private const string FirstName = "  jane";
    private const string SanitizedFirstName = "Jane";
    private const string LastName = "blo  ";
    private const string SanitizedLastName = "Blo";
    private const string Street = "111, HOLLYWOOD Blvd  ";
    private const string SanitizedStreet = "111, Hollywood Blvd";
    private const string City = "SAN Francisco  ";
    private const string SanitizedCity = "San Francisco";
    private const string ZipCode = "H1s 7k1  ";
    private const string SanitizedZipCode = "H1S 7K1";

    private readonly UserBuilder _userBuilder;

    private readonly Member _member;

    public MemberTests()
    {
        _userBuilder = new UserBuilder();
        var memberBuilder = new MemberBuilder();

        var user = _userBuilder.WithEmail(AnyEmail).Build();
        _member = memberBuilder.WithFirstName(FirstName).WithLastName(LastName).WithUser(user).Build();
    }

    [Fact]
    public void WhenSetUser_ThenMemberUserIsSameAsGivenUser()
    {
        // Arrange
        var user = _userBuilder.Build();

        // Act
        _member.SetUser(user);

        // Assert
        _member.User.ShouldBe(user);
    }

    [Fact]
    public void OnCreated_ThenSetUserAsMemberUser()
    {
        // Arrange
        var user = _userBuilder.Build();

        // Act
        _member.OnCreated(user);

        // Assert
        _member.User.ShouldBe(user);
    }

    [Fact]
    public void WhenActivate_ThenSetDeletedToNull()
    {
        // Arrange
        _member.Deleted = Instant.MaxValue;

        // Act
        _member.Activate();

        // Assert
        _member.Deleted.ShouldBeNull();
    }

    [Fact]
    public void WhenActivate_ThenSetDeletedByToNull()
    {
        // Arrange
        _member.DeletedBy = AnyEmail;

        // Act
        _member.Activate();

        // Assert
        _member.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenSanitizeFirstName()
    {
        // Arrange
        _member.SetFirstName(FirstName);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.FirstName.ShouldBe(SanitizedFirstName);
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenSanitizeLastName()
    {
        // Arrange
        _member.SetLastName(LastName);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.LastName.ShouldBe(SanitizedLastName);
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenSanitizeStreet()
    {
        // Arrange
        _member.SetStreet(Street);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.Street.ShouldBe(SanitizedStreet);
    }

    [Fact]
    public void GivenNullStreet_WhenSanitizeForSaving_ThenReturnNull()
    {
        // Arrange
        _member.SetStreet(null);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.Street.ShouldBeNull();
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenSanitizeCity()
    {
        // Arrange
        _member.SetCity(City);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.City.ShouldBe(SanitizedCity);
    }

    [Fact]
    public void GivenNullCity_WhenSanitizeForSaving_ThenReturnNull()
    {
        // Arrange
        _member.SetCity(null);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.City.ShouldBeNull();
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenSanitizeZipCode()
    {
        // Arrange
        _member.SetZipCode(ZipCode);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.ZipCode.ShouldBe(SanitizedZipCode);
    }

    [Fact]
    public void GivenNullZipCode_WhenSanitizeForSaving_ThenReturnNull()
    {
        // Arrange
        _member.SetZipCode(null);

        // Act
        _member.SanitizeForSaving();

        // Assert
        _member.ZipCode.ShouldBeNull();
    }
}