namespace Application.Exceptions.Members;

public class MemberWithEmailAlreadyExistsException: Exception
{
    public MemberWithEmailAlreadyExistsException(string message) : base(message) { }
}