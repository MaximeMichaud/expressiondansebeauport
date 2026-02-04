using Domain.Extensions;
using Infrastructure.ExternalApis.Azure.Exceptions;

namespace Tests.Infrastructure.ExternalApis.Azure.Exceptions;

public class AzureApiExceptionTests
{
    private const string SomeMessage = "Could not upload file to azure blob.";

    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeAzureApiException()
    {
        // Arrange
        var azureApiException = new AzureApiException(SomeMessage);

        // Act
        var actual = azureApiException.ErrorObject();

        // Arrange
        actual.ErrorType.ShouldBe("AzureApiException");
    }

    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var azureApiException = new AzureApiException(SomeMessage);

        // Act
        var actual = azureApiException.ErrorObject();

        // Arrange
        actual.ErrorMessage.ShouldBe(SomeMessage);
    }
}