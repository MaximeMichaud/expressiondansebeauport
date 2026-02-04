using FluentValidation.TestHelper;
using Web.Features.Members.Books.GetBook;

namespace Tests.Web.Features.Admins.Books.GetBook;

public class GetBookValidatorTests
{
    private readonly GetBookRequest _request;
    private readonly GetBookValidator _validator;

    public GetBookValidatorTests()
    {
        _validator = new GetBookValidator();
        _request = new GetBookRequest
        {
            Id = Guid.NewGuid(),
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
    public void GivenEmptytId_WhenValidate_ThenReturnError()
    {
        // Arrange
        _request.Id = Guid.Empty;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Id);
    }
}