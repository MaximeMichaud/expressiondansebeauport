using Application.Extensions;
using FastEndpoints;
using FluentValidation.Results;
using Web.Extensions;

namespace Tests.Web.Extensions;

public class ValidationFailureExceptionExtensionsTests
{
    private const string AnyErrorCode = "ErrorCode";
    private const string AnyErrorMessage = "Some error message";
    
    [Fact]
    public void GivenExceptionWithNullListOfFailures_WhenErrorObjects_ThenReturnEmptyList()
    {
        // Arrange
        var exception = new ValidationFailureException();

        // Act
        var errors = exception.ErrorObjects();
        
        errors.ShouldBeEmpty();
    }
    
    [Fact]
    public void GivenExceptionWithFailures_WhenErrorObjects_ThenReturnListOfError()
    {
        // Arrange
        var failure = new ValidationFailure { ErrorCode = AnyErrorCode, ErrorMessage = AnyErrorMessage };
        var exception = new ValidationFailureException(failure.IntoList(), AnyErrorMessage);

        // Act
        var errors = exception.ErrorObjects();
        
        errors.ShouldNotBeEmpty();
    }
    
    [Fact]
    public void GivenExceptionWithFailures_WhenErrorObjects_ThenReturnedErrorTypeShouldBeFailureErrorCode()
    {
        // Arrange
        var failure = new ValidationFailure { ErrorCode = AnyErrorCode, ErrorMessage = AnyErrorMessage };
        var exception = new ValidationFailureException(failure.IntoList(), AnyErrorMessage);

        // Act
        var errors = exception.ErrorObjects();
        
        errors.ShouldContain(x => x.ErrorType == AnyErrorCode);
    }
    
    [Fact]
    public void GivenExceptionWithFailures_WhenErrorObjects_ThenReturnedErrorMessageShouldBeFailureErrorMessage()
    {
        // Arrange
        var failure = new ValidationFailure { ErrorCode = AnyErrorCode, ErrorMessage = AnyErrorMessage };
        var exception = new ValidationFailureException(failure.IntoList(), AnyErrorMessage);

        // Act
        var errors = exception.ErrorObjects();
        
        errors.ShouldContain(x => x.ErrorType == AnyErrorCode);
    }
}