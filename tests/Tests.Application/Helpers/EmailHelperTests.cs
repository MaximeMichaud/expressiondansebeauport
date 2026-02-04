using Application.Helpers;

namespace Tests.Application.Helpers;

public class EmailHelperTests
{

    [Theory]
    [InlineData("me@gmail.com")]
    [InlineData("me@a.ca")]
    [InlineData("me.spk@a.ca")]
    public void GivenValidEmail_WhenIsValidEmail_ThenReturnTrue(string email)
    {
        // Act
        var response = email.IsValidEmail();

        // Assert
        response.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t")]
    [InlineData("abc")]
    [InlineData("123456")]
    [InlineData("alo.com")]
    [InlineData(null)]
    public void GivenInvalidEmail_WhenIsValidEmail_ThenReturnFalse(string? email)
    {
        // Act
        var response = email.IsValidEmail();

        // Assert
        response.ShouldBeFalse();
    }
}