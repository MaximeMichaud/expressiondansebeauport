namespace Infrastructure.Repositories.Users.Exceptions;

public class UpdateUserException : Exception
{
    public UpdateUserException(string message) : base(message) { }
}