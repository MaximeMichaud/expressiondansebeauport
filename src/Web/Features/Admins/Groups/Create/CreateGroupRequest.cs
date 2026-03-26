namespace Web.Features.Admins.Groups.Create;

public class CreateGroupRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Season { get; set; } = null!;
    public string? InviteCode { get; set; }
    public string? ImageUrl { get; set; }
}
