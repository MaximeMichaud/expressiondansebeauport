namespace Application.Exceptions.Users;

public class UserNotFoundException: Exception
{
    public UserNotFoundException(string message) : base(message) { }
}