using Application.Exceptions.Books;
using Domain.Entities.Books;
using Infrastructure.Repositories.Books;
using Slugify;
using Tests.Common.Builders;
using Tests.Infrastructure.Fixtures;
using Tests.Infrastructure.TestCollections;

namespace Tests.Infrastructure.Repositories.Books;

[Collection(InfrastructureTestCollection.NAME)]
public class BookRepositoryTests
{
    private const string OtherIsbn = "651-5-94823-947-1";
    private const string UnusedIsbn = "947-3-23759-293-2";
    private const string OtherUnusedIsbn = "947-3-23759-293-8";

    private readonly PersistenceTestFixture _testFixture;

    private readonly BookBuilder _bookBuilder;

    private readonly BookRepository _bookRepository;

    public BookRepositoryTests(PersistenceTestFixture testFixture)
    {
        _testFixture = testFixture;
        _bookBuilder = new BookBuilder();
        _bookRepository = new BookRepository(_testFixture.DbContext, new SlugHelper());
    }

    [Fact]
    public async Task GivenBooksInDatabase_WhenGetAll_ThenReturnAllBooksOfDatabase()
    {
        // Arrange
        var book = await GivenBookInDatabase();

        // Act
        var books = _bookRepository.GetAll();

        // Assert
        books.ShouldContain(x => x.Id == book.Id);
    }

    [Fact]
    public async Task GivenNoBooksInDatabase_WhenGetAll_ThenReturnEmptyList()
    {
        // Arrange
        _testFixture.DbContext.Books.RemoveRange(_testFixture.DbContext.Books);
        await _testFixture.DbContext.SaveChangesAsync();

        // Act
        var books = _bookRepository.GetAll();

        // Assert
        books.ShouldBeEmpty();
    }

    [Fact]
    public void GivenNoBooksWithIdExists_WhenFindById_ThenThrowBookNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & assert
        Assert.Throws<BookNotFoundException>(() => _bookRepository.FindById(id));
    }

    [Fact]
    public async Task GivenBooksWithIdExists_WhenFindById_ThenReturnMatchingBook()
    {
        // Arrange
        var expectedBook = await GivenBookInDatabase();

        // Act
        var actualBook = _bookRepository.FindById(expectedBook.Id);

        // Assert
        actualBook.Id.ShouldBe(expectedBook.Id);
    }

    [Fact]
    public async Task GivenBookWithSameIsbnAlreadyExists_WhenCreateBook_ThenThrowBookWithIsbnAlreadyExistsException()
    {
        // Arrange
        var existingBook = await GivenBookInDatabase();
        var book = _bookBuilder.WithIsbn(existingBook.Isbn).Build();

        // Act & assert
        await Assert.ThrowsAsync<BookWithIsbnAlreadyExistsException>(async () =>
            await _bookRepository.CreateBook(book));
    }

    [Fact]
    public async Task GivenNoBookWithSameIsbnAlreadyExists_WhenCreateBook_ThenSaveBookToDatabase()
    {
        // Arrange
        var book = _bookBuilder.WithIsbn(OtherUnusedIsbn).Build();

        // Act
        await _bookRepository.CreateBook(book);

        // Assert
        _testFixture.DbContext.Books.Any(x => x.Isbn == OtherUnusedIsbn).ShouldBeTrue();
    }

    [Fact]
    public async Task GivenOtherBookWithSameIsbnAlreadyExists_WhenUpdateBook_ThenThrowBookWithIsbnAlreadyExistsException()
    {
        // Arrange
        var existingBook = await GivenBookInDatabase();
        var otherExistingBook = await GivenBookInDatabase(OtherIsbn);
        otherExistingBook.SetIsbn(existingBook.Isbn);

        // Act & assert
        await Assert.ThrowsAsync<BookWithIsbnAlreadyExistsException>(async () =>
            await _bookRepository.UpdateBook(otherExistingBook));
    }

    [Fact]
    public async Task GivenNoBookWithSameIsbnAlreadyExists_WhenUpdateBook_ThenSaveBookToDatabase()
    {
        // Arrange
        var existingBook = await GivenBookInDatabase();
        existingBook.SetIsbn(UnusedIsbn);

        // Act
        await _bookRepository.UpdateBook(existingBook);

        // Assert
        _testFixture.DbContext.Books.Any(x => x.Isbn == UnusedIsbn).ShouldBeTrue();
    }

    [Fact]
    public async Task GivenNoBookWithIdExists_WhenDeleteBookWithId_ThenThrowBookNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        // Act & assert
        await Assert.ThrowsAsync<BookNotFoundException>(async () =>
            await _bookRepository.DeleteBookWithId(bookId));
    }

    [Fact]
    public async Task GivenBookWithIdExists_WhenDeleteBookWithId_ThenDeleteBook()
    {
        // Arrange
        var book = await GivenBookInDatabase();

        // Act
        await _bookRepository.DeleteBookWithId(book.Id);

        // Assert
        _testFixture.DbContext.Books.Any(x => x.Id == book.Id).ShouldBeFalse();
    }

    private async Task<Book> GivenBookInDatabase(string? isbn = null)
    {
        var book =  _bookBuilder.WithIsbn(isbn).Build();
        _testFixture.DbContext.Books.Add(book);
        await _testFixture.DbContext.SaveChangesAsync();
        return book;
    }
}