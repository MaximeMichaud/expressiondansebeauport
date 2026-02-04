using Application.Exceptions.Books;
using Application.Interfaces.FileStorage;
using Application.Interfaces.Services.Books;
using Domain.Entities.Books;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Books;

public class BookUpdateService : IBookUpdateService
{
    private readonly IBookRepository _bookRepository;
    private readonly IFileStorageApiConsumer _fileStorageApiConsumer;

    public BookUpdateService(
        IBookRepository bookRepository,
        IFileStorageApiConsumer fileStorageApiConsumer)
    {
        _bookRepository = bookRepository;
        _fileStorageApiConsumer = fileStorageApiConsumer;
    }

    public async Task UpdateBook(Book book, IFormFile? cardImage)
    {
        var existingBook = _bookRepository.FindById(book.Id);
        if (existingBook == null)
            throw new BookNotFoundException($"Could not find book with id {book.Id}.");

        book = await UpdateCardImage(existingBook, book, cardImage);

        await _bookRepository.UpdateBook(book);
    }

    private async Task<Book> UpdateCardImage(Book existingBook, Book book, IFormFile? cardImage)
    {
        if (cardImage == null)
        {
            book.SetCardImageUrl(existingBook.CardImage);
            return book;
        }
        await _fileStorageApiConsumer.DeleteFileWithUrl(existingBook.CardImage);
        book.SetCardImageUrl(await _fileStorageApiConsumer.UploadFileAsync(cardImage));
        return book;
    }
}