using Domain.Common;

namespace Domain.Entities;

public class Poll : Entity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public string Question { get; private set; } = null!;
    public bool AllowMultipleAnswers { get; private set; }

    public ICollection<PollOption> Options { get; private set; } = new List<PollOption>();

    public void SetPost(Post post)
    {
        Post = post;
        PostId = post.Id;
    }

    public void SetQuestion(string question) => Question = question;
    public void SetAllowMultipleAnswers(bool allow) => AllowMultipleAnswers = allow;
}
