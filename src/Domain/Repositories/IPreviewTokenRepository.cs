using Domain.Entities;

namespace Domain.Repositories;

public interface IPreviewTokenRepository
{
    Task<PreviewToken?> FindByToken(string token);
    Task Create(PreviewToken previewToken);
    Task DeleteExpired();
}
