using Domain.Entities.Authentication;
using Domain.Helpers;
using Domain.Repositories;
using Infrastructure.Repositories.Authentication.Exceptions;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Authentication;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly GarneauTemplateDbContext _context;

    public RefreshTokenRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> FindByToken(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
        if (refreshToken == null)
            throw new RefreshTokenNotFoundException($"Could not find refresh token with token {token}");
        return refreshToken;
    }

    public async Task<bool> RefreshTokenWithTokenExists(string token)
    {
        return await _context.RefreshTokens.AnyAsync(t => t.Token == token);
    }

    public async Task<bool> ValidRefreshTokenWithTokenExists(string token)
    {
        return await _context.RefreshTokens.AnyAsync(t => t.Token == token && t.ExpiresAt >= InstantHelper.GetLocalNow());
    }

    public async Task<RefreshToken> Create(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);

        // Delete user's previous token(s)
        var userTokens = FindForUserWithId(refreshToken.UserId);
        _context.RefreshTokens.RemoveRange(userTokens.Where(x => x.Token != refreshToken.Token));
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task DeleteRefreshTokenWithToken(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (refreshToken == null)
            return;

        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }

    private IEnumerable<RefreshToken> FindForUserWithId(Guid userId)
    {
        return _context.RefreshTokens.Where(t => t.UserId == userId);
    }
}