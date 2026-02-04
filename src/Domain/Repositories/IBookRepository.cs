using Domain.Entities.Books;

namespace Domain.Repositories;

public interface IBookRepository
{
    List<Book> GetAll();
    Book FindById(Guid id);
    Task CreateBook(Book book);
    Task UpdateBook(Book book);
    Task DeleteBookWithId(Guid id);
}