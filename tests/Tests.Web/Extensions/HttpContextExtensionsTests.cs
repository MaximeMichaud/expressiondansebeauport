using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Tests.Web.TestCollections;
using Web.Extensions;

namespace Tests.Web.Extensions;

[Collection(WebTestCollection.NAME)]
public class HttpContextExtensionsTests
{
    [Fact]
    public void GivenHttpContextHasUser_WhenIsAuthenticated_ThenReturnTrue()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity(new List<Claim>(), CookieAuthenticationDefaults.AuthenticationScheme);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claimsIdentity) };

        // Act
        var isAuthenticated = httpContext.IsAuthenticated();
        
        // Assert
        isAuthenticated.ShouldBeTrue();
    }
    
    [Fact]
    public void GivenHttpContextHasNoUser_WhenIsAuthenticated_ThenReturnFalse()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var isAuthenticated = httpContext.IsAuthenticated();
        
        // Assert
        isAuthenticated.ShouldBeFalse();
    }
}