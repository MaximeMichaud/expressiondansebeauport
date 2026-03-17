namespace Web.Features.Admins.Members.GetAll;

public class GetAllMembersRequest
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
