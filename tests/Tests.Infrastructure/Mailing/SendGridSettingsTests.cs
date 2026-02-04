using Infrastructure.Mailing;

namespace Tests.Infrastructure.Mailing;

public class SendGridSettingsTests
{
    [Fact]
    public void GivenAnyApiKey_WhenNew_ThenApiKeyPropertyHasSameValue()
    {
        // Arrange
        const string apiKey = "anyApiKey";
        
        // Act
        var sendGridSettings = new SendGridSettings { ApiKey = apiKey };
        
        // Assert
        sendGridSettings.ApiKey.ShouldBe(apiKey);
    }
}