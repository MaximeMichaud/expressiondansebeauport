using FluentValidation.TestHelper;
using Web.Features.Members.Books.EditBook;

namespace Tests.Web.Features.Admins.Books.EditBook;

public class EditBookValidatorTests
{
    private const string NameFr = "Guide de la réglementation en copropriété";
    private const string NameEn = "Guide to condominium regulations";
    private const string DescriptionFr = "On vise, en copropriété, à assurer aux occupant un milieu de vie paisible.";
    private const string DescriptionEn = "We aim, in co-ownership, to provide occupants with a peaceful living environment.";
    private const string Isbn = "978-2-89689-559-5";
    private const string Author = "Christine Gagnon, Yves Papineau";
    private const string Editor = "Wilson & Lafleur";
    private const int YearOfPublication = 2023;
    private const int NumberOfPages = 346;
    private const decimal Price = 20;

    private readonly EditBookRequest _request;
    private readonly EditBookValidator _validator;

    public EditBookValidatorTests()
    {
        _validator = new EditBookValidator();
        _request = new EditBookRequest
        {
            Id = Guid.NewGuid(),
            NameFr = NameFr,
            NameEn = NameEn,
            DescriptionFr = DescriptionFr,
            DescriptionEn = DescriptionEn,
            Isbn = Isbn,
            Author = Author,
            Editor = Editor,
            YearOfPublication = YearOfPublication,
            NumberOfPages = NumberOfPages,
            Price = Price
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

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceNameFr_WhenValidate_ThenReturnError(string? nameFr)
    {
        // Arrange
        _request.NameFr = nameFr!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.NameFr);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceNameEn_WhenValidate_ThenReturnError(string? nameEn)
    {
        // Arrange
        _request.NameEn = nameEn!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.NameEn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceDescriptionFr_WhenValidate_ThenReturnError(string? descriptionFr)
    {
        // Arrange
        _request.DescriptionFr = descriptionFr!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.DescriptionFr);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceDescriptionEn_WhenValidate_ThenReturnError(string? descriptionEn)
    {
        // Arrange
        _request.DescriptionEn = descriptionEn!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.DescriptionEn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceIsbn_WhenValidate_ThenReturnError(string? isbn)
    {
        // Arrange
        _request.Isbn = isbn!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceAuthor_WhenValidate_ThenReturnError(string? author)
    {
        // Arrange
        _request.Author = author!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    public void GivenNullEmptyOrWhitespaceEditor_WhenValidate_ThenReturnError(string? editor)
    {
        // Arrange
        _request.Editor = editor!;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Editor);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    public void GivenYearOfPublicationIsLowerOrEqualToZero_WhenValidate_ThenReturnError(int yearOfPublication)
    {
        // Arrange
        _request.YearOfPublication = yearOfPublication;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.YearOfPublication);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    public void GivenNullEmptyOrWhitespaceNumberOfPages_WhenValidate_ThenReturnError(int numberOfPages)
    {
        // Arrange
        _request.NumberOfPages = numberOfPages;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.NumberOfPages);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    public void GivenNullEmptyOrWhitespacePrice_WhenValidate_ThenReturnError(decimal price)
    {
        // Arrange
        _request.Price = price;

        // Act
        var validationResult = _validator.TestValidate(_request);

        // Assert
        validationResult.ShouldHaveValidationErrorFor(x => x.Price);
    }
}