using Domain.Entities;

namespace Domain.Repositories;

public interface ICommentRepository
{
    Task Add(Comment comment);
    Task<Comment?> FindById(Guid id, bool asNoTracking = true);
    Task<List<Comment>> GetByPostId(Guid postId, int skip, int take);
    Task Update(Comment comment);
}
