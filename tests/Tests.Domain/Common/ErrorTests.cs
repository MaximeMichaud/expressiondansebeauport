using Domain.Common;

namespace Tests.Domain.Common;

public class ErrorTests
{
    private const string ANY_ERROR_TYPE = "UpdateUserException";
    private const string ANY_ERROR_MESSAGE = "Could not update user.";

    [Fact]
    public void WhenNew_ThenErrorTypeIsDefault()
    {
        // Act
        var error = new Error();

        // Assert
        error.ErrorType.ShouldBe(null);
    }

    [Fact]
    public void WhenNew_ThenErrorMessageIsDefault()
    {
        // Act
        var error = new Error();

        // Assert
        error.ErrorMessage.ShouldBe(null);
    }

    [Fact]
    public void GivenErrorType_WhenNew_ThenErrorMessageIsSameAsGivenErrorType()
    {
        // Act
        var error = new Error(ANY_ERROR_TYPE, ANY_ERROR_MESSAGE);

        // Assert
        error.ErrorType.ShouldBe(ANY_ERROR_TYPE);
    }

    [Fact]
    public void GivenErrorMessage_WhenNew_ThenErrorMessageIsSameAsGivenErrorMessage()
    {
        // Act
        var error = new Error(ANY_ERROR_TYPE, ANY_ERROR_MESSAGE);

        // Assert
        error.ErrorMessage.ShouldBe(ANY_ERROR_MESSAGE);
    }
}