using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Persistence;

namespace Infrastructure.Repositories.Messaging;

public class ConversationRepository : IConversationRepository
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMemberRepository _memberRepository;

    public ConversationRepository(GarneauTemplateDbContext context, IMemberRepository memberRepository)
    {
        _context = context;
        _memberRepository = memberRepository;
    }

    public async Task<Conversation?> FindOrCreate(Guid memberAId, Guid memberBId)
    {
        var (smallerId, largerId) = memberAId.CompareTo(memberBId) < 0
            ? (memberAId, memberBId)
            : (memberBId, memberAId);

        var existing = await _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.Member).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c =>
                c.ParticipantAMemberId == smallerId &&
                c.ParticipantBMemberId == largerId);

        if (existing != null)
            return existing;

        var memberA = _memberRepository.FindById(smallerId, asNoTracking: false);
        var memberB = _memberRepository.FindById(largerId, asNoTracking: false);
        if (memberA == null || memberB == null)
            return null;

        var now = InstantHelper.GetLocalNow();
        var conversation = new Conversation();
        conversation.SetCreatedAt(now);
        conversation.SetParticipants(memberA, memberB);

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var participantA = new ConversationParticipant();
        participantA.SetConversation(conversation);
        participantA.SetMember(memberA);
        participantA.SetJoinedAt(now);
        _context.ConversationParticipants.Add(participantA);

        var participantB = new ConversationParticipant();
        participantB.SetConversation(conversation);
        participantB.SetMember(memberB);
        participantB.SetJoinedAt(now);
        _context.ConversationParticipants.Add(participantB);

        await _context.SaveChangesAsync();
        return conversation;
    }

    public async Task<Conversation?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.Member).ThenInclude(m => m.User)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Conversation>> GetForMember(Guid memberId, int skip, int take)
    {
        return await _context.Conversations
            .AsNoTracking()
            .Where(c => c.ParticipantAMemberId == memberId || c.ParticipantBMemberId == memberId)
            .Include(c => c.ParticipantA)
            .Include(c => c.ParticipantB)
            .Include(c => c.Participants).ThenInclude(p => p.Member)
            .Include(c => c.Messages.OrderByDescending(m => m.Created).Take(1))
                .ThenInclude(m => m.SenderMember)
            .OrderByDescending(c => c.Messages.Max(m => (Instant?)m.Created))
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCount(Guid memberId)
    {
        return await _context.ConversationParticipants
            .AsNoTracking()
            .Where(cp => cp.MemberId == memberId)
            .Where(cp => cp.Conversation.Messages.Any(m =>
                m.SenderMemberId != memberId &&
                (cp.LastReadAt == null || m.Created > cp.LastReadAt)))
            .CountAsync();
    }

    public async Task Add(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
    }
}
