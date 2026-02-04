namespace Application.Services.Members.Exceptions;

public class GetAuthenticatedMemberException : Exception
{
    public GetAuthenticatedMemberException(string message) : base(message) { }
}