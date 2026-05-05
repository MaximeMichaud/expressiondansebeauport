using Application.Services.Notifications.Models;

namespace Tests.Application.Services.Notifications.Models;

public class ForgotPasswordNotificationModelTests
{
    private const string AnyLink = "www.google.com";
    private const string AnyEmail = "garneau@spektrummedia.com";
    private const string AnyLocale = "fr";

    [Fact]
    public void GivenAnyEmail_WhenNewForgotPasswordNotificationModel_ThenDestinationEmailShouldBeSameAsGivenEmail()
    {
        // Arrange & act
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Assert
        forgotPasswordNotificationModel.Destination.ShouldBe(AnyEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespaceEmail_WhenNewForgotPasswordNotificationModel_ThenThrowArgumentException(string? email)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => new ForgotPasswordNotificationModel(email!, AnyLocale, AnyLink));
    }

    [Fact]
    public void GivenAnyLocale_WhenNewForgotPasswordNotificationModel_ThenLocaleShouldBeSameAsGivenLocale()
    {
        // Arrange & act
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Assert
        forgotPasswordNotificationModel.Locale.ShouldBe(AnyLocale);
    }


    [Fact]
    public void GivenAnyLink_WhenNewForgotPasswordNotificationModel_ThenLinkShouldBeSameAsGivenLink()
    {
        // Arrange & act
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Assert
        forgotPasswordNotificationModel.Link.ShouldBe(AnyLink);
    }
}