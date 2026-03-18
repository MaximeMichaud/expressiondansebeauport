using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
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
            .OrderByDescending(m => m.Created)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task MarkAsRead(Guid conversationId, Guid memberId)
    {
        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.MemberId == memberId);

        if (participant == null) return;

        participant.SetLastReadAt(InstantHelper.GetLocalNow());
        await _context.SaveChangesAsync();
    }

    public async Task SaveChanges() => await _context.SaveChangesAsync();
}
