using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IMediaFileRepository
{
    PaginatedList<MediaFile> GetAllPaginated(int pageIndex, int pageSize);
    MediaFile? FindById(Guid id);
    bool Exists(string fileName);
    Task Create(MediaFile mediaFile);
    Task Delete(MediaFile mediaFile);
    Task Update(MediaFile mediaFile);
}
