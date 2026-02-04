using Application.Services.Notifications.Models;
using Tests.Common.Extensions;

namespace Tests.Application.Services.Notifications.Models;

public class TwoFactorAuthenticationNotificationModelTests
{
    private const string AnyLocale = "fr";
    private const string EnTemplateId = "d-0ba3cba8d527436dbebe4ee440104d77";
    private const string FrTemplateId = "d-b6b894b660614a289e1d3e0e1cc81c17";
    private const string AnyEmail = "john.doe@gmail.com";
    private const string AnyCode = "123456";

    [Fact]
    public void GivenAnyEmail_WhenNewTwoFactorAuthenticationNotificationModel_ThenDestinationEmailShouldBeSameAsGivenEmail()
    {
        // Arrange & act
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, AnyLocale, AnyCode);

        // Assert
        twoFactorAuthenticationNotificationModel.Destination.ShouldBe(AnyEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespaceEmail_WhenTwoFactorAuthenticationNotificationModel_ThenThrowArgumentException(string? email)
    {
        // Act & assert
        Assert.Throws<ArgumentException>(() => new TwoFactorAuthenticationNotificationModel(email!, AnyLocale, AnyCode));
    }

    [Fact]
    public void GivenAnyLocale_WhenNewTwoFactorAuthenticationNotificationModel_ThenLocaleShouldBeSameAsGivenLocale()
    {
        // Arrange & act
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, AnyLocale, AnyCode);

        // Assert
        twoFactorAuthenticationNotificationModel.Locale.ShouldBe(AnyLocale);
    }


    [Fact]
    public void GivenAnyCode_WhenNewTwoFactorAuthenticationNotificationModel_ThenCodeShouldBeSameAsGivenCode()
    {
        // Arrange & act
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, AnyLocale, AnyCode);

        // Assert
        twoFactorAuthenticationNotificationModel.Code.ShouldBe(AnyCode);
    }

    [Fact]
    public void WhenLocaleIsEnglish_WhenTemplateId_ThenReturnEnglishTemplateId()
    {
        // Arrange
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, "en", AnyCode);

        // Act
        var templateId = twoFactorAuthenticationNotificationModel.TemplateId();

        // Assert
        templateId.ShouldBe(EnTemplateId);
    }

    [Fact]
    public void WhenLocaleIsFrench_WhenTemplateId_ThenReturnFrenchTemplateId()
    {
        // Arrange
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, "fr", AnyCode);

        // Act
        var templateId = twoFactorAuthenticationNotificationModel.TemplateId();

        // Assert
        templateId.ShouldBe(FrTemplateId);
    }

    [Fact]
    public void WhenTemplateData_ThenReturnTwoFactorCodeAsCode()
    {
        // Arrange
        var twoFactorAuthenticationNotificationModel = new TwoFactorAuthenticationNotificationModel(AnyEmail, AnyLocale, AnyCode);

        // Act
        var templateData = twoFactorAuthenticationNotificationModel.TemplateData();

        // Assert
        templateData.GetStringValueOfProperty("Code").ShouldBe(AnyCode);
    }
}