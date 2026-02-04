namespace Application.Helpers.Exceptions;

public class UnsupportedCultureException : Exception
{
    public UnsupportedCultureException(string message) : base(message) { }
}