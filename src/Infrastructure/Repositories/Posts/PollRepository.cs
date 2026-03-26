using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Posts;

public class PollRepository : IPollRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PollRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Poll poll)
    {
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
    }

    public async Task<Poll?> FindByPostId(Guid postId)
    {
        return await _context.Polls
            .AsNoTracking()
            .Include(p => p.Options.OrderBy(o => o.SortOrder)).ThenInclude(o => o.Votes)
            .FirstOrDefaultAsync(p => p.PostId == postId);
    }

    public async Task<bool> HasVoted(Guid pollOptionId, Guid memberId)
    {
        return await _context.PollVotes.AnyAsync(v => v.PollOptionId == pollOptionId && v.MemberId == memberId);
    }

    public async Task<bool> HasVotedOnPoll(Guid pollId, Guid memberId)
    {
        return await _context.PollVotes
            .AnyAsync(v => v.PollOption.PollId == pollId && v.MemberId == memberId);
    }

    public async Task AddVote(PollVote vote)
    {
        _context.PollVotes.Add(vote);
        await _context.SaveChangesAsync();
    }

    public async Task<PollOption?> FindOptionById(Guid optionId)
    {
        return await _context.PollOptions
            .Include(o => o.Poll)
            .FirstOrDefaultAsync(o => o.Id == optionId);
    }
}
