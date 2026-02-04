using Application.Helpers;

namespace Tests.Application.Helpers;

public class PostalCodeHelperTests
{

    [Theory]
    [InlineData("a1a2a3")]
    [InlineData("a1a 2a3")]
    [InlineData("A1A 2A3")]
    [InlineData("A1A2A3")]
    public void GivenValidPostalCode_WhenIsValidPostalCode_ThenReturnTrue(string postalCode)
    {
        // Act
        var response = postalCode.IsValidPostalCode();

        // Assert
        response.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t")]
    [InlineData("abc")]
    [InlineData("123456")]
    [InlineData(null)]
    public void GivenInvalidPostalCode_WhenIsValidPostalCode_ThenReturnTrue(string? postalCode)
    {
        // Act
        var response = postalCode.IsValidPostalCode();

        // Assert
        response.ShouldBeFalse();
    }
}