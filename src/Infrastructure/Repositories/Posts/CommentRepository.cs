using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.Posts;

public class CommentRepository : ICommentRepository
{
    private readonly GarneauTemplateDbContext _context;

    public CommentRepository(GarneauTemplateDbContext context) => _context = context;

    public async Task Add(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<Comment?> FindById(Guid id, bool asNoTracking = true)
    {
        var query = _context.Comments
            .Include(c => c.AuthorMember).ThenInclude(m => m.User)
            .AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Comment>> GetByPostId(Guid postId, int skip, int take)
    {
        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .Include(c => c.AuthorMember).ThenInclude(m => m.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(c => c.Created)
            .Skip(skip).Take(take)
            .ToListAsync();
    }

    public async Task Update(Comment comment)
    {
        if (_context.Entry(comment).State == EntityState.Detached)
            _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }
}
