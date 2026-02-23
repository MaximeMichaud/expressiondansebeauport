using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.SiteSettings;

public class SiteSettingsRepository : ISiteSettingsRepository
{
    private readonly GarneauTemplateDbContext _context;

    public SiteSettingsRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.SiteSettings> Get()
    {
        var settings = await _context.SiteSettings
            .Include(s => s.LogoMediaFile)
            .Include(s => s.FaviconMediaFile)
            .FirstOrDefaultAsync();

        if (settings is not null) return settings;

        settings = new Domain.Entities.SiteSettings();
        _context.SiteSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task Update(Domain.Entities.SiteSettings settings)
    {
        _context.SiteSettings.Update(settings);
        await _context.SaveChangesAsync();
    }
}
