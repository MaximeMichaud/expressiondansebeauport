namespace Web.Features.Admins.Groups.AssignProfessor;

public class AssignProfessorRequest
{
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
}
