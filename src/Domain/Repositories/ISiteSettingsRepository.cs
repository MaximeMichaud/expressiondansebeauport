using Domain.Entities;

namespace Domain.Repositories;

public interface ISiteSettingsRepository
{
    Task<SiteSettings> Get();
    Task Update(SiteSettings settings);
}
