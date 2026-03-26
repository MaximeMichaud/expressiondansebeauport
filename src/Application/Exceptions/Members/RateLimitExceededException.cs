namespace Application.Exceptions.Members;

public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message) { }
}
