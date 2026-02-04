using Microsoft.AspNetCore.Http;
using Web.Cookies;

namespace Tests.Web.Cookies;

public class CookieHelperTests
{
    private const string AnyCookieName = "lang";
    private const string AnyCookieValue = "fr";
    private const string AnyCookieDomain = "https://groupemenagestar.ca";

    private readonly HttpContext _httpContext;

    public CookieHelperTests()
    {
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public void WhenGetCookieValue_ThenReturnValue()
    {
        // Act
        var cookieValue = _httpContext.GetCookieValue(AnyCookieName);

        // Assert
        cookieValue.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenNullEmptyOrWhiteSpaceCookieName_WhenGetCookieValue_ThenReturnEmpty(string? cookieName)
    {
        // Act
        var cookieValue = _httpContext.GetCookieValue(cookieName!);

        // Assert
        cookieValue.ShouldBeEmpty();
    }

    [Fact]
    public void WhenSetCookieValue_ThenAddCookieValueToResponse()
    {
        // Act
        _httpContext.Response.SetCookieValue(AnyCookieName, AnyCookieValue, AnyCookieDomain, true, true);

        // Assert
        _httpContext.Response.Cookies.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenNullEmptyOrWhiteSpaceCookieName_WhenSetCookieValue_ThenDontAddCookie(string? cookieName)
    {
        // Act
        _httpContext.Response.SetCookieValue(cookieName!, AnyCookieValue, AnyCookieDomain, false, false);

        // Assert
        _httpContext.Response.Cookies.ShouldNotBeNull();
    }
}