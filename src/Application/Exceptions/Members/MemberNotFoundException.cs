namespace Application.Exceptions.Members;

public class MemberNotFoundException: Exception
{
    public MemberNotFoundException(string message) : base(message) { }
}