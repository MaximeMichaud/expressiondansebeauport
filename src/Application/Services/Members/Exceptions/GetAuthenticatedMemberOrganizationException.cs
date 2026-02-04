namespace Application.Services.Members.Exceptions;

public class GetAuthenticatedMemberOrganizationException : Exception
{
    public GetAuthenticatedMemberOrganizationException(string message) : base(message) { }
}