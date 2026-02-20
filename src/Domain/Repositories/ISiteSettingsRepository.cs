using Domain.Entities;

namespace Domain.Repositories;

public interface ISiteSettingsRepository
{
    SiteSettings Get();
    Task Update(SiteSettings settings);
}
