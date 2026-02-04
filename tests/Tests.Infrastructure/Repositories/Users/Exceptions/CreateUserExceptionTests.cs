using Domain.Extensions;
using Infrastructure.Repositories.Users.Exceptions;

namespace Tests.Infrastructure.Repositories.Users.Exceptions;

public class CreateUserExceptionTests
{
    private const string AnyMessage = "Could not create user.";

    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeCreateUserException()
    {
        // Arrange
        var createUserException = new CreateUserException(AnyMessage);

        // Act
        var actual = createUserException.ErrorObject();

        // Assert
        actual.ErrorType.ShouldBe("CreateUserException");
    }

    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var createUserException = new CreateUserException(AnyMessage);

        // Act
        var actual = createUserException.ErrorObject();

        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}