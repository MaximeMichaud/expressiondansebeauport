namespace Application.Exceptions.Books;

public class BookWithIsbnAlreadyExistsException: Exception
{
    public BookWithIsbnAlreadyExistsException(string message) : base(message) { }
}