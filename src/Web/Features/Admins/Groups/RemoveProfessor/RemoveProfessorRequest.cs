namespace Web.Features.Admins.Groups.RemoveProfessor;

public class RemoveProfessorRequest
{
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
}
