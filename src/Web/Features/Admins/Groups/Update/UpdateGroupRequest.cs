namespace Web.Features.Admins.Groups.Update;

public class UpdateGroupRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Season { get; set; } = null!;
    public string? ImageUrl { get; set; }
}
