namespace Infrastructure.Repositories.Users.Exceptions;

public class CreateUserException : Exception
{
    public CreateUserException(string message) : base(message) { }
}