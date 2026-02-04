using Application.Services.Notifications.Models;
using Infrastructure.Mailing;
using Infrastructure.Mailing.Mapping;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tests.Common.Mapping;

namespace Tests.Infrastructure.Mailing;

public class SendGridMessageFactoryTests
{
    private const string AnyLink = "www.google.com";
    private const string AnyEmail = "garneau@spektrummedia.com";
    private const string AnyLocale = "fr";
    private const string AnyName = "Garneau Template";

    private readonly Mock<IWebHostEnvironment> _webHostEnvironment;

    private readonly SendGridMessageFactory _sendGridMessageFactory;

    public SendGridMessageFactoryTests()
    {
        _webHostEnvironment = new Mock<IWebHostEnvironment>();
        var mailingSettings = new MailingSettings
        {
            FromAddress = AnyEmail,
            FromName = AnyName,
            ToAddressForDevelopment = AnyEmail
        };
        var mailingSettingsOptions = new Mock<IOptions<MailingSettings>>();
        mailingSettingsOptions.Setup(x => x.Value).Returns(mailingSettings);

        var mapper = new MapperBuilder().WithProfile<MailingMappingProfile>().Build();
        _sendGridMessageFactory =
            new SendGridMessageFactory(_webHostEnvironment.Object, mailingSettingsOptions.Object, mapper);
    }

    [Fact]
    public void WhenCreateFromModel_ThenReturnSendGridMessage()
    {
        // Arrange
        _webHostEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Staging);
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Act
        var msg = _sendGridMessageFactory.CreateFromModel(model);

        // Assert
        msg.ShouldNotBeNull();
    }

    [Fact]
    public void WhenCreateFromModel_ThenUseMailingSettingsFromAddressAsFromEmail()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Act
        var msg = _sendGridMessageFactory.CreateFromModel(model);

        // Assert
        msg.From.Email.ShouldBe(AnyEmail);
    }

    [Fact]
    public void WhenCreateFromModel_ThenUseMailingSettingsFromNameAsFromName()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Act
        var msg = _sendGridMessageFactory.CreateFromModel(model);

        // Assert
        msg.From.Name.ShouldBe(AnyName);
    }

    [Fact]
    public void WhenCreateFromModel_ThenUseModelTemplateIdAsTemplateId()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);

        // Act
        var msg = _sendGridMessageFactory.CreateFromModel(model);

        // Assert
        msg.TemplateId.ShouldBe(model.TemplateId());
    }
}