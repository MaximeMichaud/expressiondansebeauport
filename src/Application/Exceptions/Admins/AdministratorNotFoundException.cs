namespace Application.Exceptions.Admins;

public class AdministratorNotFoundException: Exception
{
    public AdministratorNotFoundException(string message) : base(message) { }
}