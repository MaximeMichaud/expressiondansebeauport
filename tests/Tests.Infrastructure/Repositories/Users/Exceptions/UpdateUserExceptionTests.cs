using Domain.Extensions;
using Infrastructure.Repositories.Users.Exceptions;

namespace Tests.Infrastructure.Repositories.Users.Exceptions;

public class UpdateUserExceptionTests
{
    private const string AnyMessage = "Could not update user.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeUpdateUserException()
    {
        // Arrange
        var updateUserException = new UpdateUserException(AnyMessage);
        
        // Act
        var actual = updateUserException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("UpdateUserException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var updateUserException = new UpdateUserException(AnyMessage);
        
        // Act
        var actual = updateUserException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}