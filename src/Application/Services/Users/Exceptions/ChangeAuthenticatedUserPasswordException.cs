namespace Application.Services.Users.Exceptions;

public class ChangeAuthenticatedUserPasswordException: Exception
{
    public ChangeAuthenticatedUserPasswordException(string message) : base(message) { }
}