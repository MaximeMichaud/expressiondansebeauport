namespace Infrastructure.Repositories.Users.Exceptions;

public class RoleNotFoundException: Exception
{
    public RoleNotFoundException(string message) : base(message) { }
}