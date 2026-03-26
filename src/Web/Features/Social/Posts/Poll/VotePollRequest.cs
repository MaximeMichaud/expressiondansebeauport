namespace Web.Features.Social.Posts.Poll;

public class VotePollRequest
{
    public Guid PostId { get; set; }
    public Guid PollOptionId { get; set; }
}
