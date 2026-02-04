using Application.Interfaces.FileStorage;
using Application.Interfaces.Services.Books;
using Domain.Entities.Books;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Books;

public class BookCreationService : IBookCreationService
{
    private readonly IBookRepository _bookRepository;
    private readonly IFileStorageApiConsumer _fileStorageApiConsumer;

    public BookCreationService(
        IBookRepository bookRepository,
        IFileStorageApiConsumer fileStorageApiConsumer)
    {
        _bookRepository = bookRepository;
        _fileStorageApiConsumer = fileStorageApiConsumer;
    }

    public async Task CreateBook(Book book, IFormFile cardImage)
    {
        book.SetCardImageUrl(await _fileStorageApiConsumer.UploadFileAsync(cardImage));

        await _bookRepository.CreateBook(book);
    }
}