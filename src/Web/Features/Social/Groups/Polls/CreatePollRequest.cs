namespace Web.Features.Social.Groups.Polls;

public class CreatePollRequest
{
    public Guid GroupId { get; set; }
    public string Question { get; set; } = null!;
    public List<string> Options { get; set; } = new();
    public bool AllowMultipleAnswers { get; set; }
}
