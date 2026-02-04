using Domain.Entities.Books;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.Books;

public interface IBookCreationService
{
    Task CreateBook(Book book, IFormFile cardImage);
}