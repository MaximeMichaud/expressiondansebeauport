using Domain.Extensions;
using Infrastructure.Repositories.Users.Exceptions;

namespace Tests.Infrastructure.Repositories.Users.Exceptions;

public class RoleNotFoundExceptionTests
{
    private const string AnyMessage = "Could not find role with given name.";

    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeRoleNotFoundException()
    {
        // Arrange
        var roleNotFoundException = new RoleNotFoundException(AnyMessage);

        // Act
        var actual = roleNotFoundException.ErrorObject();

        // Assert
        actual.ErrorType.ShouldBe("RoleNotFoundException");
    }

    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var roleNotFoundException = new RoleNotFoundException(AnyMessage);

        // Act
        var actual = roleNotFoundException.ErrorObject();

        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}