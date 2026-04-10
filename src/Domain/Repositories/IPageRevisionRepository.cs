using Domain.Entities;

namespace Domain.Repositories;

public interface IPageRevisionRepository
{
    List<PageRevision> GetByPageId(Guid pageId);
    PageRevision? FindById(Guid id);
    PageRevision? GetLatestByPageId(Guid pageId, RevisionType? type = null);
    int GetNextRevisionNumber(Guid pageId);
    Task Create(PageRevision revision);
    Task DeleteOldRevisions(Guid pageId, int keepCount);
    Task DeleteAutosave(Guid pageId);
    Task<PageRevision?> GetAutosave(Guid pageId);
}
