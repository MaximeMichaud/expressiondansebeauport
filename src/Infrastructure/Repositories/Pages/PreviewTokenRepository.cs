using Domain.Entities;
using Domain.Repositories;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Pages;

public class PreviewTokenRepository : IPreviewTokenRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PreviewTokenRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task<PreviewToken?> FindByToken(string token)
    {
        return await _context.PreviewTokens
            .Include(t => t.Page)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task Create(PreviewToken previewToken)
    {
        _context.PreviewTokens.Add(previewToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpired()
    {
        var now = InstantHelper.GetLocalNow();
        var expired = _context.PreviewTokens
            .Where(t => t.ExpiresAt <= now)
            .ToList();

        if (expired.Count > 0)
        {
            _context.PreviewTokens.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}
