using Application.Services.Notifications.Models;
using Tests.Common.Extensions;

namespace Tests.Application.Services.Notifications.Models;

public class ForgotPasswordNotificationModelTests
{
    private const string AnyLink = "www.google.com";
    private const string AnyEmail = "garneau@spektrummedia.com";
    private const string AnyLocale = "fr";
    private const string EnTemplateId = "d-6bceb5f892064a7b95cc03fe16b45943";
    private const string FrTemplateId = "d-ccea0bf1594048259d41fb52c2c23614";

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

    [Fact]
    public void WhenLocaleIsEnglish_WhenTemplateId_ThenReturnEnglishTemplateId()
    {
        // Arrange
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, "en", AnyLink);

        // Act
        var templateId = forgotPasswordNotificationModel.TemplateId();

        // Assert
        templateId.ShouldBe(EnTemplateId);
    }

    [Fact]
    public void WhenLocaleIsFrench_WhenTemplateId_ThenReturnFrenchTemplateId()
    {
        // Arrange
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, "fr", AnyLink);

        // Act
        var templateId = forgotPasswordNotificationModel.TemplateId();

        // Assert
        templateId.ShouldBe(FrTemplateId);
    }

    [Fact]
    public void WhenTemplateData_ThenReturnLinkAsButtonUrl()
    {
        // Arrange
        var forgotPasswordNotificationModel = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Act
        var templateData = forgotPasswordNotificationModel.TemplateData();

        // Assert
        templateData.GetStringValueOfProperty("ButtonUrl").ShouldBe(AnyLink);
    }
}