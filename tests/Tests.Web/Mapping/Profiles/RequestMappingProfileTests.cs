using AutoMapper;
using Domain.Common;
using Domain.Entities.Books;
using Tests.Common.Mapping;
using Tests.Web.TestCollections;
using Web.Dtos;
using Web.Features.Members.Books.CreateBook;
using Web.Features.Members.Books.EditBook;
using Web.Mapping.Profiles;

namespace Tests.Web.Mapping.Profiles;

[Collection(WebTestCollection.NAME)]
public class RequestMappingProfileTests
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

    private readonly IMapper _mapper = new MapperBuilder()
        .WithProfile<RequestMappingProfile>()
        .Build();

    [Fact]
    public void GivenTranslatableStringDto_WhenMap_ThenReturnTranslatableStringMappedCorrectly()
    {
        // Arrange
        var translatableStringDto = new TranslatableStringDto
        {
            Fr = NameFr,
            En = NameEn
        };

        // Act
        var translatableString = _mapper.Map<TranslatableString>(translatableStringDto);

        // Assert
        translatableString.Fr.ShouldBe(NameFr);
        translatableString.En.ShouldBe(NameEn);
    }

    [Fact]
    public void GivenTranslatableString_WhenMap_ThenReturnTranslatableStringDtoMappedCorrectly()
    {
        // Arrange
        var translatableString = new TranslatableString
        {
            Fr = NameFr,
            En = NameEn
        };

        // Act
        var translatableStringDto = _mapper.Map<TranslatableStringDto>(translatableString);

        // Assert
        translatableStringDto.Fr.ShouldBe(NameFr);
        translatableStringDto.En.ShouldBe(NameEn);
    }

    [Fact]
    public void GivenCreateBookRequest_WhenMap_ThenReturnBookMappedCorrectly()
    {
        // Arrange
        var createBookRequest = new CreateBookRequest
        {
            NameFr = NameFr,
            NameEn = NameEn,
            Price = Price,
            DescriptionFr = DescriptionFr,
            DescriptionEn = DescriptionEn,
            Isbn = Isbn,
            Author = Author,
            Editor = Editor,
            YearOfPublication = YearOfPublication,
            NumberOfPages = NumberOfPages
        };

        // Act
        var book = _mapper.Map<Book>(createBookRequest);

        // Assert
        book.NameFr.ShouldBe(NameFr);
        book.NameEn.ShouldBe(NameEn);
        book.DescriptionFr.ShouldBe(DescriptionFr);
        book.DescriptionEn.ShouldBe(DescriptionEn);
        book.Isbn.ShouldBe(Isbn);
        book.Author.ShouldBe(Author);
        book.Editor.ShouldBe(Editor);
        book.YearOfPublication.ShouldBe(YearOfPublication);
        book.NumberOfPages.ShouldBe(NumberOfPages);
    }

    [Fact]
    public void GivenEditBookRequest_WhenMap_ThenReturnBookMappedCorrectly()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var editBookRequest = new EditBookRequest
        {
            Id = bookId,
            NameFr = NameFr,
            NameEn = NameEn,
            Price = Price,
            DescriptionFr = DescriptionFr,
            DescriptionEn = DescriptionEn,
            Isbn = Isbn,
            Author = Author,
            Editor = Editor,
            YearOfPublication = YearOfPublication,
            NumberOfPages = NumberOfPages
        };

        // Act
        var book = _mapper.Map<Book>(editBookRequest);

        // Assert
        book.Id.ShouldBe(bookId);
        book.NameFr.ShouldBe(NameFr);
        book.NameEn.ShouldBe(NameEn);
        book.DescriptionFr.ShouldBe(DescriptionFr);
        book.DescriptionEn.ShouldBe(DescriptionEn);
        book.Isbn.ShouldBe(Isbn);
        book.Author.ShouldBe(Author);
        book.Editor.ShouldBe(Editor);
        book.YearOfPublication.ShouldBe(YearOfPublication);
        book.NumberOfPages.ShouldBe(NumberOfPages);
    }
}