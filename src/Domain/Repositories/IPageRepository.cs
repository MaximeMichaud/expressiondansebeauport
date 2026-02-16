using Domain.Entities;

namespace Domain.Repositories;

public interface IPageRepository
{
    List<Page> GetAll();
    Page FindById(Guid id);
    Page FindBySlug(string slug);
    Task Update(Page page);
}
