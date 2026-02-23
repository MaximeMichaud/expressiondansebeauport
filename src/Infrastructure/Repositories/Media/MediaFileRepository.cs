using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Media;

public class MediaFileRepository : IMediaFileRepository
{
    private readonly GarneauTemplateDbContext _context;

    public MediaFileRepository(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public PaginatedList<MediaFile> GetAllPaginated(int pageIndex, int pageSize)
    {
        var query = _context.MediaFiles.AsNoTracking();
        var items = query.OrderByDescending(x => x.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<MediaFile>(items, query.Count());
    }

    public MediaFile? FindById(Guid id)
    {
        return _context.MediaFiles.FirstOrDefault(x => x.Id == id);
    }

    public bool Exists(string fileName)
    {
        return _context.MediaFiles.Any(x => x.FileName == fileName);
    }

    public async Task Create(MediaFile mediaFile)
    {
        _context.MediaFiles.Add(mediaFile);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(MediaFile mediaFile)
    {
        _context.MediaFiles.Remove(mediaFile);
        await _context.SaveChangesAsync();
    }

    public async Task Update(MediaFile mediaFile)
    {
        _context.MediaFiles.Update(mediaFile);
        await _context.SaveChangesAsync();
    }
}
