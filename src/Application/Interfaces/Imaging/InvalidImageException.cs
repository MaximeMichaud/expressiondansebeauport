namespace Application.Interfaces.Imaging;

public class InvalidImageException : Exception
{
    public InvalidImageException(string message) : base(message) { }
    public InvalidImageException(string message, Exception inner) : base(message, inner) { }
}
