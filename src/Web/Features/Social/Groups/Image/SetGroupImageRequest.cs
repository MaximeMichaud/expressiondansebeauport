namespace Web.Features.Social.Groups.Image;

public class SetGroupImageRequest
{
    public Guid GroupId { get; set; }
    public string ImageUrl { get; set; } = null!;
}
