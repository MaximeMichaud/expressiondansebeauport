using Application.Helpers.Exceptions;
using Domain.Extensions;

namespace Tests.Application.Helpers.Exceptions;

public class UnsupportedCultureExceptionTests
{
    private const string AnyMessage = "Culture is not supported.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeUnsupportedCultureException()
    {
        // Arrange
        var unsupportedCultureException = new UnsupportedCultureException(AnyMessage);
        
        // Act
        var actual = unsupportedCultureException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("UnsupportedCultureException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var unsupportedCultureException = new UnsupportedCultureException(AnyMessage);
        
        // Act
        var actual = unsupportedCultureException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}