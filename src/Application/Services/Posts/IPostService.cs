using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Posts;

public class PostMediaItem
{
    public string DisplayUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}

public interface IPostService
{
    Task<Post> CreatePost(Guid? groupId, Guid authorMemberId, string content, PostType type, IReadOnlyList<PostMediaItem> media);
    Task<Post> CreateAnnouncement(Guid authorMemberId, string content, IReadOnlyList<PostMediaItem> media);
    Task UpdateAnnouncement(Guid postId, string content, IReadOnlyList<PostMediaItem> media);
    Task<Post> CreatePollPost(
        Guid groupId,
        Guid authorMemberId,
        string question,
        IReadOnlyList<string> options,
        bool allowMultipleAnswers);
    Task DeletePost(Guid postId);
    Task PinPost(Guid postId, Guid groupId);
    Task<bool> ToggleLike(Guid postId, Guid memberId);
    Task AddComment(Guid postId, Guid authorMemberId, string content);
    Task DeleteComment(Guid commentId);
    Task RecordView(Guid postId, Guid memberId);
    Task VoteOnPoll(Guid pollOptionId, Guid memberId);
    Task<PaginatedResult<Post>> GetGroupFeed(Guid groupId, int page);
    Task<PaginatedResult<Post>> GetAnnouncements(int page);
    Task<Post?> GetPostById(Guid postId);
    Task<PaginatedResult<Comment>> GetComments(Guid postId, int page);
}
