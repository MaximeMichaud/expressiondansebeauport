namespace Application.Services.Push;

public class PushPayload
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Url { get; set; } = "/social";
    public string? Tag { get; set; }
    public Guid? GroupId { get; set; }
}
