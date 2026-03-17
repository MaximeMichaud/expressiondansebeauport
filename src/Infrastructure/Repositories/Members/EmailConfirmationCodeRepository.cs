using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Members;

public class EmailConfirmationCodeRepository : IEmailConfirmationCodeRepository
{
    private readonly GarneauTemplateDbContext _context;

    public EmailConfirmationCodeRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(EmailConfirmationCode code)
    {
        _context.EmailConfirmationCodes.Add(code);
        await _context.SaveChangesAsync();
    }

    public async Task<EmailConfirmationCode?> GetLatestForUser(Guid userId)
    {
        return await _context.EmailConfirmationCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task Update(EmailConfirmationCode code)
    {
        if (_context.Entry(code).State == EntityState.Detached)
            _context.EmailConfirmationCodes.Update(code);
        await _context.SaveChangesAsync();
    }
}
