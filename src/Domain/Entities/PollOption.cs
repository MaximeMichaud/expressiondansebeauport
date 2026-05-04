using Domain.Common;

namespace Domain.Entities;

public class PollOption : Entity
{
    public Guid PollId { get; private set; }
    public Poll Poll { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public int SortOrder { get; private set; }

    public ICollection<PollVote> Votes { get; private set; } = new List<PollVote>();

    public void SetPoll(Poll poll)
    {
        Poll = poll;
        PollId = poll.Id;
    }

    public void SetText(string text) => Text = text;
    public void SetSortOrder(int order) => SortOrder = order;

    public static PollOption Create(Poll poll, string text, int sortOrder)
    {
        var option = new PollOption();
        option.SetPoll(poll);
        option.SetText(text);
        option.SetSortOrder(sortOrder);
        return option;
    }
}
