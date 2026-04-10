using Domain.Entities;

namespace Domain.Repositories;

public interface IPollRepository
{
    Task Add(Poll poll);
    Task<Poll?> FindByPostId(Guid postId);
    Task<bool> HasVoted(Guid pollOptionId, Guid memberId);
    Task<bool> HasVotedOnPoll(Guid pollId, Guid memberId);
    Task AddVote(PollVote vote);
    Task RemoveVote(Guid pollOptionId, Guid memberId);
    Task RemoveVotesForPoll(Guid pollId, Guid memberId);
    Task<PollOption?> FindOptionById(Guid optionId);
}
