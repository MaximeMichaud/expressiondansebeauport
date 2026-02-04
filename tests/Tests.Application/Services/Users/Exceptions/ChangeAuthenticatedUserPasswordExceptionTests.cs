using Application.Services.Users.Exceptions;
using Domain.Extensions;

namespace Tests.Application.Services.Users.Exceptions;

public class ChangeAuthenticatedUserPasswordExceptionTests
{
    private const string AnyMessage = "Could not change authenticated user password.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeChangeAuthenticatedUserPasswordException()
    {
        // Arrange
        var changeAuthenticatedUserPasswordException = new ChangeAuthenticatedUserPasswordException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedUserPasswordException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("ChangeAuthenticatedUserPasswordException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var changeAuthenticatedUserPasswordException = new ChangeAuthenticatedUserPasswordException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedUserPasswordException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}