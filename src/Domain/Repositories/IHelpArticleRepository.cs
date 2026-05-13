using Domain.Entities;

namespace Domain.Repositories;

public interface IHelpArticleRepository
{
    Task<HelpArticle?> GetById(Guid id);
    Task<HelpArticle?> GetBySlug(string slug);
    Task<HelpArticle?> GetByRouteHint(string routeName);
    Task<List<HelpArticle>> GetAll(HelpCategory? category = null, bool? isPublished = null);
    Task<bool> SlugExists(string slug, Guid? excludeId = null);
    Task Create(HelpArticle article);
    Task Update(HelpArticle article);
    Task Delete(Guid id);
}
