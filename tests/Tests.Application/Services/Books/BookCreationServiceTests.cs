using Application.Interfaces.FileStorage;
using Application.Services.Books;
using Domain.Entities.Books;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Tests.Common.Builders;

namespace Tests.Application.Services.Books;

public class BookCreationServiceTests
{
    private readonly IFormFile _anyCardImageFormFile;

    private readonly BookBuilder _bookBuilder;

    private readonly Mock<IBookRepository> _bookRepository;
    private readonly Mock<IFileStorageApiConsumer> _fileStorageApiConsumer;

    private readonly BookCreationService _bookCreationService;

    public BookCreationServiceTests()
    {
        _anyCardImageFormFile = new FormFile(new MemoryStream(new byte[5]), long.MaxValue,
            long.MaxValue, "card image name", "any_filename");

        _bookBuilder = new BookBuilder();

        _bookRepository = new Mock<IBookRepository>();
        _fileStorageApiConsumer = new Mock<IFileStorageApiConsumer>();

        _bookCreationService = new BookCreationService(
            _bookRepository.Object,
            _fileStorageApiConsumer.Object);
    }

    [Fact]
    public async Task WhenCreateBook_ThenUploadCardImageToFileStorage()
    {
        // Arrange
        var book = _bookBuilder.Build();

        // Act
        await _bookCreationService.CreateBook(book, _anyCardImageFormFile);

        // Assert
        _fileStorageApiConsumer.Verify(x => x.UploadFileAsync(_anyCardImageFormFile));
    }

    [Fact]
    public async Task WhenCreateBook_ThenDelegateCreatingBookToBookRepository()
    {
        // Arrange
        var book = _bookBuilder.Build();

        // Act
        await _bookCreationService.CreateBook(book, _anyCardImageFormFile);

        // Assert
        _bookRepository.Verify(x => x.CreateBook(It.IsAny<Book>()));
    }
}