using AutoMapper;
using Domain.Common;
using Microsoft.AspNetCore.Identity;
using Tests.Common.Builders;
using Tests.Common.Mapping;
using Tests.Web.TestCollections;
using Web.Constants;
using Web.Features.Members.Books;
using Web.Mapping.Profiles;

namespace Tests.Web.Mapping.Profiles;

[Collection(WebTestCollection.NAME)]
public class ResponseMappingProfileTests
{
    private const string AnyErrorDescription = "Could not create user.";

    private const string NameFr = "Guide de la réglementation en copropriété";
    private const string NameEn = "Guide to condominium regulations";
    private const string DescriptionFr = "On vise, en copropriété, à assurer aux occupant un milieu de vie paisible.";
    private const string DescriptionEn = "We aim, in co-ownership, to provide occupants with a peaceful living environment.";
    private const string Isbn = "978-2-89689-559-5";
    private const string Author = "Christine Gagnon, Yves Papineau";
    private const string Editor = "Wilson & Lafleur";
    private const string CardImage = "www.google.com";
    private const int YearOfPublication = 2023;
    private const int NumberOfPages = 346;

    private readonly BookBuilder _bookBuilder;

    private readonly IMapper _mapper;

    public ResponseMappingProfileTests()
    {
        _bookBuilder = new BookBuilder();

        _mapper = new MapperBuilder().WithProfile<ResponseMappingProfile>().Build();
    }

    [Fact]
    public void GivenSuccessfulIdentityResult_WhenMap_ThenSucceededIsTrue()
    {
        // Arrange
        var identityResult = IdentityResult.Success;

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void GivenFailedIdentityResult_WhenMap_ThenSucceededContainsErrorWithIdentityCodeAsErrorType()
    {
        // Arrange
        var error = new IdentityError { Code = IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Errors.ShouldContain(x => x.ErrorType == error.Code);
    }

    [Fact]
    public void GivenFailedIdentityResult_WhenMap_ThenSucceededContainsErrorWithIdentityErrorDescriptionAsErrorMessage()
    {
        // Arrange
        var error = new IdentityError { Description = AnyErrorDescription };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Errors.ShouldContain(x => x.ErrorMessage == AnyErrorDescription);
    }

    [Fact]
    public void GivenIdentityErrorWithCode_WhenMap_ThenErrorHasIdentityResultCodeAsErrorType()
    {
        // Arrange
        var identityError = new IdentityError { Code = IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD };

        // Act
        var error = _mapper.Map<Error>(identityError);

        // Assert
        error.ErrorType.ShouldBe(IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD);
    }

    [Fact]
    public void GivenIdentityErrorWithCode_WhenMap_ThenErrorHasIdentityResultDescriptionAsErrorMessage()
    {
        // Arrange
        var identityError = new IdentityError { Description = AnyErrorDescription };

        // Act
        var error = _mapper.Map<Error>(identityError);

        // Assert
        error.ErrorMessage.ShouldBe(AnyErrorDescription);
    }

    [Fact]
    public void GivenBook_WhenMap_ThenReturnBookDtoMappedCorrectly()
    {
        // Arrange
        var book = _bookBuilder
            .WithName(NameFr, NameEn)
            .WithDescriptions(DescriptionFr, DescriptionEn)
            .WithIsbn(Isbn)
            .WithAuthor(Author)
            .WithEditor(Editor)
            .WithCardImage(CardImage)
            .WithYearOfPublication(YearOfPublication)
            .WithNumberOfPages(NumberOfPages)
            .Build();

        // Act
        var bookDto = _mapper.Map<BookDto>(book);

        // Assert
        bookDto.Id.ShouldBe(book.Id);
        bookDto.NameFr.ShouldBe(NameFr);
        bookDto.NameEn.ShouldBe(NameEn);
        bookDto.DescriptionFr.ShouldBe(DescriptionFr);
        bookDto.DescriptionEn.ShouldBe(DescriptionEn);
        bookDto.Isbn.ShouldBe(Isbn);
        bookDto.Author.ShouldBe(Author);
        bookDto.Editor.ShouldBe(Editor);
        bookDto.CardImage.ShouldBe(CardImage);
        bookDto.YearOfPublication.ShouldBe(YearOfPublication);
        bookDto.NumberOfPages.ShouldBe(NumberOfPages);
    }
}