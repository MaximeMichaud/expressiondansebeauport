using Domain.Entities.Books;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.Books;

public interface IBookUpdateService
{
    Task UpdateBook(Book book, IFormFile? cardImage);
}