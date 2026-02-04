namespace Application.Services.Users.Exceptions;

public class ChangeAuthenticatedUserEmailException: Exception
{
    public ChangeAuthenticatedUserEmailException(string message) : base(message) { }
}