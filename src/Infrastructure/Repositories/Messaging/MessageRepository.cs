using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Persistence;

namespace Infrastructure.Repositories.Messaging;

public class MessageRepository : IMessageRepository
{
    private readonly GarneauTemplateDbContext _context;

    public MessageRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<Message?> FindById(Guid id)
    {
        return await _context.Messages
            .Include(m => m.SenderMember)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Message>> GetByConversation(Guid conversationId, int skip, int take)
    {
        return await _context.Messages
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.SenderMember).ThenInclude(s => s.User)
            .Include(m => m.Media.OrderBy(x => x.SortOrder))
            .OrderByDescending(m => m.Created)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task MarkAsRead(Guid conversationId, Guid memberId)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.MemberId == memberId);

        if (participant == null) return;

        // Find the latest message in this conversation to ensure LastReadAt is after it
        var latestMessage = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.Created)
            .Select(m => m.Created)
            .FirstOrDefaultAsync();

        // Set LastReadAt to whichever is later: now or latest message + 1 second
        var now = InstantHelper.GetLocalNow().Plus(Duration.FromSeconds(1));
        var readAt = latestMessage > now ? latestMessage.Plus(Duration.FromSeconds(1)) : now;

        participant.SetLastReadAt(readAt);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChanges() => await _context.SaveChangesAsync();
}
