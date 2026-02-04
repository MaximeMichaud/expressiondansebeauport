using Domain.Entities.Authentication;

namespace Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> FindByToken(string token);
    Task<bool> RefreshTokenWithTokenExists(string token);
    Task<bool> ValidRefreshTokenWithTokenExists(string token);
    Task<RefreshToken> Create(RefreshToken refreshToken);
    Task DeleteRefreshTokenWithToken(string token);
}