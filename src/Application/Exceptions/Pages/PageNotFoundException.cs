namespace Application.Exceptions.Pages;

public class PageNotFoundException : Exception
{
    public PageNotFoundException(string message) : base(message) { }
}
