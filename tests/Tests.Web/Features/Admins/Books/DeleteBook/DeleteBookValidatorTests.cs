using FluentValidation.TestHelper;
using Web.Features.Members.Books.DeleteBook;

namespace Tests.Web.Features.Admins.Books.DeleteBook;

public class DeleteBookValidatorTests
{
    private readonly DeleteBookRequest _request;
    private readonly DeleteBookValidator _validator;

    public DeleteBookValidatorTests()
    {
        _validator = new DeleteBookValidator();
        _request = new DeleteBookRequest
        {
            Id = Guid.NewGuid()
        };
    }
    
    [Fact]
    public void GivenValidRequest_WhenValidate_ThenReturnNoErrors()
    {
        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void GivenEmptyId_WhenValidate_ThenReturnError()
    {
        // Arrange
        _request.Id = Guid.Empty;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Id);
    }
}