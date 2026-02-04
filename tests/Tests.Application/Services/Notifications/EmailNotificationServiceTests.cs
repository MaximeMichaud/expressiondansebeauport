using Application.Interfaces.Mailing;
using Application.Services.Notifications;
using Application.Services.Notifications.Models;
using Tests.Application.TestCollections;
using Tests.Common.Builders;

namespace Tests.Application.Services.Notifications;

[Collection(ApplicationTestCollection.NAME)]
public class EmailNotificationServiceTests
{
    private const string AnyEmail = "garneau@spektrummedia.com";
    private const string AnyLink = "www.google.com";
    private const string AnyCode = "123456";

    private readonly Mock<IEmailSender> _emailSender;

    private readonly EmailNotificationService _emailNotificationService;

    private readonly UserBuilder _userBuilder;

    public EmailNotificationServiceTests()
    {
        _userBuilder = new UserBuilder();

        _emailSender = new Mock<IEmailSender>();
        _emailNotificationService = new EmailNotificationService(_emailSender.Object);
    }

    [Fact]
    public async Task GivenAnyUserAndLink_WhenSendForgotPasswordNotification_ThenSendForgotPasswordEmail()
    {
        // Arrange
        var user = _userBuilder.WithEmail(AnyEmail).Build();

        // Act
        await _emailNotificationService.SendForgotPasswordNotification(user, AnyLink);

        // Assert
        _emailSender.Verify(x => x.SendAsync(It.IsAny<ForgotPasswordNotificationModel>()));
    }

    [Fact]
    public async Task GivenAnyEmailAndCode_WhenSendTwoFactorAuthenticationCode_ThenSendTwoFactorAuthenticationNotification()
    {
        // Arrange
        var user = _userBuilder.WithEmail(AnyEmail).Build();

        // Act
        await _emailNotificationService.SendTwoFactorAuthenticationCodeNotification(user, AnyCode);

        // Assert
        _emailSender.Verify(x => x.SendAsync(It.IsAny<TwoFactorAuthenticationNotificationModel>()));
    }
}