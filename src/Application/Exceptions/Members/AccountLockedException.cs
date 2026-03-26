namespace Application.Exceptions.Members;

public class AccountLockedException : Exception
{
    public AccountLockedException(string message) : base(message) { }
}
