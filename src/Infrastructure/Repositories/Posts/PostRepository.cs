using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Posts;

public class PostRepository : IPostRepository
{
    private readonly GarneauTemplateDbContext _context;

    public PostRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }

    public async Task<Post?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.Posts
            .Include(p => p.AuthorMember).ThenInclude(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(p => p.Media.OrderBy(m => m.SortOrder))
            .Include(p => p.Reactions)
            .Include(p => p.Comments)
            .Include(p => p.Poll).ThenInclude(pl => pl!.Options.OrderBy(o => o.SortOrder)).ThenInclude(o => o.Votes)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Post>> GetByGroupId(Guid groupId, int skip, int take)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.GroupId == groupId)
            .Include(p => p.AuthorMember).ThenInclude(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(p => p.Media.OrderBy(m => m.SortOrder))
            .Include(p => p.Reactions)
            .Include(p => p.Poll).ThenInclude(pl => pl!.Options.OrderBy(o => o.SortOrder)).ThenInclude(o => o.Votes)
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.Created)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task<List<Post>> GetAnnouncements(int skip, int take)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.GroupId == null)
            .Include(p => p.AuthorMember).ThenInclude(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(p => p.Media.OrderBy(m => m.SortOrder))
            .Include(p => p.Reactions)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.Created)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task<Post?> GetPinnedPost(Guid groupId)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.GroupId == groupId && p.IsPinned);
    }

    public async Task Update(Post post)
    {
        if (_context.Entry(post).State == EntityState.Detached)
            _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasReaction(Guid postId, Guid memberId)
    {
        return await _context.PostReactions.AnyAsync(r => r.PostId == postId && r.MemberId == memberId);
    }

    public async Task<PostReaction?> FindReaction(Guid postId, Guid memberId)
    {
        return await _context.PostReactions.FirstOrDefaultAsync(r => r.PostId == postId && r.MemberId == memberId);
    }

    public async Task AddReaction(PostReaction reaction)
    {
        _context.PostReactions.Add(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveReaction(PostReaction reaction)
    {
        _context.PostReactions.Remove(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasView(Guid postId, Guid memberId)
    {
        return await _context.PostViews.AnyAsync(v => v.PostId == postId && v.MemberId == memberId);
    }

    public async Task AddView(PostView view)
    {
        _context.PostViews.Add(view);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetReactionCount(Guid postId)
    {
        return await _context.PostReactions.CountAsync(r => r.PostId == postId);
    }

    public async Task<int> GetCommentCount(Guid postId)
    {
        return await _context.Comments.CountAsync(c => c.PostId == postId);
    }
}
