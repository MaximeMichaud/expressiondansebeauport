namespace Application.Services.Admins.Exceptions;

public class GetAuthenticatedAdminException : Exception
{
    public GetAuthenticatedAdminException(string message) : base(message) { }
}