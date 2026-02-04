namespace Application.Services.Users.Exceptions;

public class ChangeAuthenticatedPhoneNumberException: Exception
{
    public ChangeAuthenticatedPhoneNumberException(string message) : base(message) { }
}