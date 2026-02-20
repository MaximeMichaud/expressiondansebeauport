using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IPageRepository
{
    PaginatedList<Page> GetAllPaginated(int pageIndex, int pageSize, PageStatus? status = null);
    List<Page> GetPublished();
    Page? FindById(Guid id);
    Page? FindBySlug(string slug);
    bool SlugExists(string slug, Guid? excludeId = null);
    Task Create(Page page);
    Task Update(Page page);
    Task Delete(Page page);
}
