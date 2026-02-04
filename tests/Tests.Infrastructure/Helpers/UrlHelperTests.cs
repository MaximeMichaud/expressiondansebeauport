using Infrastructure.Helpers;

namespace Tests.Infrastructure.Helpers;

public class UrlHelperTests
{
    private const string AnyBaseUrl = "https://localhost:7101";
    
    [Fact]
    public void GivenNoQueryParameters_WhenBuildUriWithQueryParameters_ThenReturnBaseUrlOnly()
    {
        // Arrange
        var queryParameters = new List<KeyValuePair<string, string>>();

        // Act
        var uri = UrlHelper.BuildUriWithQueryParameters(AnyBaseUrl, queryParameters);
        
        // Assert
        uri.ShouldBe(AnyBaseUrl);
    }
    
    [Fact]
    public void GivenQueryParameters_WhenBuildUriWithQueryParameters_ThenReturnUrlStartingWithBaseUrl()
    {
        // Arrange
        var queryParameters = new List<KeyValuePair<string, string>>
        {
            new("page", "0"),
            new("perPage", "100")
        };
        
        // Act
        var uri = UrlHelper.BuildUriWithQueryParameters(AnyBaseUrl, queryParameters);
        
        // Assert
        uri.ShouldStartWith(AnyBaseUrl);
    }
    
    [Fact]
    public void GivenQueryParameters_WhenBuildUriWithQueryParameters_TheReturnUrlDoesNotEndWithAmpersand()
    {
        // Arrange
        var queryParameters = new List<KeyValuePair<string, string>>
        {
            new("page", "0"),
            new("perPage", "100")
        };
        
        // Act
        var uri = UrlHelper.BuildUriWithQueryParameters(AnyBaseUrl, queryParameters);
        
        // Assert
        uri.ShouldNotEndWith("&");
    }
    
    [Fact]
    public void GivenQueryParameters_WhenBuildUriWithQueryParameters_TheReturnUrlIncludesAllParameters()
    {
        // Arrange
        var queryParameters = new List<KeyValuePair<string, string>>
        {
            new("page", "0"),
            new("perPage", "100")
        };
        
        // Act
        var uri = UrlHelper.BuildUriWithQueryParameters(AnyBaseUrl, queryParameters);
        
        // Assert
        uri.ShouldBe($"{AnyBaseUrl}?page=0&perPage=100");
    }
}