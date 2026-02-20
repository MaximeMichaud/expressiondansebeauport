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

    public Domain.Entities.SiteSettings Get()
    {
        var settings = _context.SiteSettings
            .Include(s => s.LogoMediaFile)
            .Include(s => s.FaviconMediaFile)
            .FirstOrDefault();

        if (settings is not null) return settings;

        settings = new Domain.Entities.SiteSettings();
        _context.SiteSettings.Add(settings);
        _context.SaveChanges();
        return settings;
    }

    public async Task Update(Domain.Entities.SiteSettings settings)
    {
        _context.SiteSettings.Update(settings);
        await _context.SaveChangesAsync();
    }
}
