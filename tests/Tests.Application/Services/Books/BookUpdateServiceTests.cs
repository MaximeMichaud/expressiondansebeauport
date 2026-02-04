using Application.Exceptions.Books;
using Application.Interfaces.FileStorage;
using Application.Services.Books;
using Domain.Entities.Books;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Tests.Common.Builders;

namespace Tests.Application.Services.Books;

public class BookUpdateServiceTests
{
    private const string ExistingCardImageUrl = "www.google.com";
    private const string NewCardImageUrl = "blob.azure.com";

    private readonly IFormFile _anyCardImageFormFile;

    private readonly BookBuilder _bookBuilder;

    private readonly Mock<IBookRepository> _bookRepository;
    private readonly Mock<IFileStorageApiConsumer> _fileStorageApiConsumer;

    private readonly BookUpdateService _bookUpdateService;

    public BookUpdateServiceTests()
    {
        _anyCardImageFormFile = new FormFile(new MemoryStream(new byte[5]), long.MaxValue,
            long.MaxValue, "card image name", "any_filename");

        _bookBuilder = new BookBuilder();

        _bookRepository = new Mock<IBookRepository>();
        _fileStorageApiConsumer = new Mock<IFileStorageApiConsumer>();

        _bookUpdateService = new BookUpdateService(
            _bookRepository.Object,
            _fileStorageApiConsumer.Object);
    }

    [Fact]
    public async Task GivenNoBookWithIdExists_WhenUpdateBook_ThenThrowBookNotFoundException()
    {
        // Arrange
        var book = _bookBuilder.Build();

        // Act & assert
        await Assert.ThrowsAsync<BookNotFoundException>(async () =>
            await _bookUpdateService.UpdateBook(book, _anyCardImageFormFile));
    }

    [Fact]
    public async Task GivenCardImageIsNull_WhenUpdateBook_ThenUseExistingBookCardImageForBookToUpdate()
    {
        // Arrange
        Book? updatedBook = null;
        var existingBook = _bookBuilder.WithCardImage(ExistingCardImageUrl).Build();
        GivenBookWithIdExists(existingBook);
        _bookRepository.Setup(x => x.UpdateBook(It.IsAny<Book>())).Callback<Book>(bookParam => updatedBook = bookParam);
        var book = _bookBuilder.Build();

        // Act
        await _bookUpdateService.UpdateBook(book, null);

        // Assert
        updatedBook?.CardImage.ShouldBe(ExistingCardImageUrl);
    }

    [Fact]
    public async Task GivenCardImageIsNotNull_WhenUpdateBook_ThenDeleteExistingBookCardImageFromFileStorage()
    {
        // Arrange
        var book = _bookBuilder.WithCardImage(ExistingCardImageUrl).Build();
        GivenBookWithIdExists(book);

        // Act
        await _bookUpdateService.UpdateBook(book, _anyCardImageFormFile);

        // Assert
        _fileStorageApiConsumer.Verify(x => x.DeleteFileWithUrl(ExistingCardImageUrl));
    }

    [Fact]
    public async Task GivenCardImageIsNotNull_WhenUpdateBook_ThenUploadNewCardImageToFileStorage()
    {
        // Arrange
        var book = _bookBuilder.WithCardImage(ExistingCardImageUrl).Build();
        GivenBookWithIdExists(book);

        // Act
        await _bookUpdateService.UpdateBook(book, _anyCardImageFormFile);

        // Assert
        _fileStorageApiConsumer.Verify(x => x.UploadFileAsync(_anyCardImageFormFile));
    }

    [Fact]
    public async Task GivenCardImageIsNotNull_WhenUpdateBook_ThenBookIsUpdatedWithNewCardImageUrl()
    {
        // Arrange
        Book? updatedBook = null;
        var book = _bookBuilder.WithCardImage(ExistingCardImageUrl).Build();
        GivenBookWithIdExists(book);
        _fileStorageApiConsumer.Setup(x => x.UploadFileAsync(_anyCardImageFormFile)).ReturnsAsync(NewCardImageUrl);
        _bookRepository.Setup(x => x.UpdateBook(It.IsAny<Book>())).Callback<Book>(bookParam => updatedBook = bookParam);

        // Act
        await _bookUpdateService.UpdateBook(book, _anyCardImageFormFile);

        // Assert
        updatedBook?.CardImage.ShouldBe(NewCardImageUrl);
    }

    [Fact]
    public async Task WhenUpdateBook_ThenDelegateUpdatingBookToBookRepository()
    {
        // Arrange
        var book = _bookBuilder.Build();
        GivenBookWithIdExists(book);

        // Act
        await _bookUpdateService.UpdateBook(book, null);

        // Assert
        _bookRepository.Setup(x => x.UpdateBook(It.IsAny<Book>()));
    }

    private void GivenBookWithIdExists(Book book)
    {
        _bookRepository
            .Setup(x => x.FindById(It.IsAny<Guid>()))
            .Returns(book);
    }
}