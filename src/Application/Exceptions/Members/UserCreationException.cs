namespace Application.Exceptions.Members;

public class UserCreationException : Exception
{
    public UserCreationException(string message) : base(message) { }
}
