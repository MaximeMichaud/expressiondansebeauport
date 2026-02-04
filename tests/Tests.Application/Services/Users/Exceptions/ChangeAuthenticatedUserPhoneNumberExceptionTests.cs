using Application.Services.Users.Exceptions;
using Domain.Extensions;

namespace Tests.Application.Services.Users.Exceptions;

public class ChangeAuthenticatedUserPhoneNumberExceptionTests
{
    private const string AnyMessage = "Could not change authenticated user phone number.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeChangeAuthenticatedPhoneNumberException()
    {
        // Arrange
        var changeAuthenticatedPhoneNumberException = new ChangeAuthenticatedPhoneNumberException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedPhoneNumberException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("ChangeAuthenticatedPhoneNumberException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var changeAuthenticatedPhoneNumberException = new ChangeAuthenticatedPhoneNumberException(AnyMessage);
        
        // Act
        var actual = changeAuthenticatedPhoneNumberException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}