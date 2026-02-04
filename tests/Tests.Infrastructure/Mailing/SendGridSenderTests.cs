using System.Net;
using System.Text;
using System.Text.Json;
using Application.Services.Notifications.Models;
using Infrastructure.Mailing;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Tests.Infrastructure.Mailing;

public class SendGridSenderTests
{
    private const string AnyLink = "www.google.com";
    private const string AnyEmail = "garneau@spektrummedia.com";
    private const string AnyLocale = "fr";
    private const string AnyErrorMessage = "Error message";

    private readonly Mock<ISendGridClient> _sendGridClient;

    private readonly SendGridSender _sendGridSender;

    public SendGridSenderTests()
    {
        var logger = new Mock<ILogger<SendGridSender>>();
        _sendGridClient = new Mock<ISendGridClient>();
        _sendGridSender = new SendGridSender(logger.Object, _sendGridClient.Object, new Mock<ISendGridMessageFactory>().Object);
    }

    [Fact]
    public async Task GivenSendingEmailFails_WhenSendAsync_ThenReturnSuccessfulFalse()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);
        var errors = new List<SendGridError> { new() { Message = AnyErrorMessage } };
        _sendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.BadRequest, BuildSendGridResponseWithErrors(errors), null));

        // Act
        var responseDto = await _sendGridSender.SendAsync(model);

        // Assert
        responseDto.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenSendingEmailFails_WhenSendAsync_ThenReturnListOfErrorMessages()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);
        var errors = new List<SendGridError> { new() { Message = AnyErrorMessage } };
        _sendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.BadRequest, BuildSendGridResponseWithErrors(errors), null));

        // Act
        var responseDto = await _sendGridSender.SendAsync(model);

        // Assert
        responseDto.Errors.Select(x => x.ErrorMessage).ShouldContain(AnyErrorMessage);
    }

    [Fact]
    public async Task GivenSendingEmailWorks_WhenSendAsync_ThenReturnSuccessfulTrue()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);
        _sendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, null, null));

        // Act
        var responseDto = await _sendGridSender.SendAsync(model);

        // Assert
        responseDto.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenSendingEmailWorks_WhenSendAsync_ThenReturnEmptyErrorList()
    {
        // Arrange
        var model = new ForgotPasswordNotificationModel(AnyEmail, AnyLocale, AnyLink);
        _sendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, null, null));

        // Act
        var responseDto = await _sendGridSender.SendAsync(model);

        // Assert
        responseDto.Errors.ShouldBeEmpty();
    }


    private HttpContent BuildSendGridResponseWithErrors(List<SendGridError> sendGridErrors)
    {
        var dictionary = new Dictionary<string, dynamic> { { "errors", sendGridErrors } };
        return new StringContent(JsonSerializer.Serialize(dictionary), Encoding.UTF8, "application/json");
    }
}