using Application.Helpers;

namespace Tests.Application.Helpers;

public class PhoneNumberHelperTests
{

    [Theory]
    [InlineData("418-123-4687")]
    public void GivenValidPhoneNumber_WhenIsValidPhoneNumber_ThenReturnTrue(string phoneNumber)
    {
        // Act
        var response = phoneNumber.IsValidPhoneNumber();

        // Assert
        response.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t")]
    [InlineData("abc")]
    [InlineData("123456")]
    [InlineData("000 000 0000")]
    [InlineData(null)]
    public void GivenInvalidPhoneNumber_WhenIsValidPhoneNumber_ThenReturnFalse(string? phoneNumber)
    {
        // Act
        var response = phoneNumber.IsValidPhoneNumber();

        // Assert
        response.ShouldBeFalse();
    }

    [Theory]
    [InlineData("418 123 4687")]
    [InlineData("(418)-123-4687")]
    [InlineData("(418)-1233-4687")]
    [InlineData("(418)-123-46872")]
    [InlineData("+1-(418)-123-4687")]
    [InlineData("+1 (418)-123-4687")]
    [InlineData("+1 418 123 4687")]
    public void GivenInvalidPhoneNumberFormat_WhenIsValidPhoneNumber_ThenReturnFalse(string phoneNumber)
    {
        // Act
        var response = phoneNumber.IsValidPhoneNumber();

        // Assert
        response.ShouldBeFalse();
    }
}