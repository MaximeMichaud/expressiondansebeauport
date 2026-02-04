namespace Application.Exceptions.Users;

public class UserWithEmailAlreadyExistsException: Exception
{
    public UserWithEmailAlreadyExistsException(string message) : base(message) { }
}