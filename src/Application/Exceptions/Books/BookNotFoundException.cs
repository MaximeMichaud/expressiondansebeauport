namespace Application.Exceptions.Books;

public class BookNotFoundException: Exception
{
    public BookNotFoundException(string message) : base(message) { }
}