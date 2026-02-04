using Application.Services.Users.Exceptions;
using Domain.Extensions;

namespace Tests.Application.Services.Users.Exceptions;

public class ChangeAuthenticatedUserEmailExceptionTests
{
    private const string AnyMessage = "Could not change authenticated user email.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeChangeAuthenticatedUserEmailException()
    {
        // Arrange
        var changeAuthenticatedUserEmailException = new ChangeAuthenticatedUserEmailException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedUserEmailException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("ChangeAuthenticatedUserEmailException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var changeAuthenticatedUserEmailException = new ChangeAuthenticatedUserEmailException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedUserEmailException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}