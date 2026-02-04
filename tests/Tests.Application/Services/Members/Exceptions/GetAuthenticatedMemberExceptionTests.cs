using Application.Services.Members.Exceptions;
using Domain.Extensions;

namespace Tests.Application.Services.Members.Exceptions;

public class GetAuthenticatedMemberExceptionTests
{
    private const string AnyMessage = "Could not get authenticated member.";
    
    [Fact]
    public void WhenErrorObject_ThenErrorTypeShouldBeGetAuthenticatedMemberException()
    {
        // Arrange
        var getAuthenticatedMemberException = new GetAuthenticatedMemberException(AnyMessage);
        
        // Act
        var actual = getAuthenticatedMemberException.ErrorObject();
        
        // Assert
        actual.ErrorType.ShouldBe("GetAuthenticatedMemberException");
    }
    
    [Fact]
    public void WhenErrorObject_ThenErrorMessageShouldBeSpecifiedMessage()
    {
        // Arrange
        var getAuthenticatedMemberException = new GetAuthenticatedMemberException(AnyMessage);
        
        // Act
        var actual = getAuthenticatedMemberException.ErrorObject();
        
        // Assert
        actual.ErrorMessage.ShouldBe(AnyMessage);
    }
}