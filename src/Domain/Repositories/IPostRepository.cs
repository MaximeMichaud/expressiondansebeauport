using Domain.Entities;

namespace Domain.Repositories;

public interface IPostRepository
{
    Task Add(Post post);
    Task<Post?> FindById(Guid id, bool asNoTracking = true);
    Task<List<Post>> GetByGroupId(Guid groupId, int skip, int take);
    Task<List<Post>> GetAnnouncements(int skip, int take);
    Task<Post?> GetPinnedPost(Guid groupId);
    Task Update(Post post);
    Task<bool> HasReaction(Guid postId, Guid memberId);
    Task<PostReaction?> FindReaction(Guid postId, Guid memberId);
    Task AddReaction(PostReaction reaction);
    Task RemoveReaction(PostReaction reaction);
    Task<bool> HasView(Guid postId, Guid memberId);
    Task AddView(PostView view);
    Task<int> GetReactionCount(Guid postId);
    Task<int> GetCommentCount(Guid postId);
}
