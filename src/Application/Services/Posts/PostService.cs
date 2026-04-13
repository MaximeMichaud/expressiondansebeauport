using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Domain.Repositories;
using NodaTime;

namespace Application.Services.Posts;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPollRepository _pollRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;

    public PostService(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IPollRepository pollRepository,
        IMemberRepository memberRepository,
        IGroupMemberRepository groupMemberRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _pollRepository = pollRepository;
        _memberRepository = memberRepository;
        _groupMemberRepository = groupMemberRepository;
    }

    public async Task<Post> CreatePost(
        Guid? groupId,
        Guid authorMemberId,
        string content,
        PostType type,
        IReadOnlyList<PostMediaItem> media)
    {
        if (media.Count > 10)
            throw new InvalidOperationException("A post cannot have more than 10 media items.");

        var member = _memberRepository.FindById(authorMemberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var effectiveType = media.Count > 0 ? PostType.Photo : type;

        var post = new Post();
        post.SetGroupId(groupId);
        post.SetAuthor(member);
        post.SetContent(content);
        post.SetType(effectiveType);

        for (var i = 0; i < media.Count; i++)
        {
            var item = media[i];
            var pm = new PostMedia();
            pm.SetPost(post);
            pm.SetMediaUrl(item.DisplayUrl);
            pm.SetThumbnailUrl(item.ThumbnailUrl);
            pm.SetOriginalUrl(item.OriginalUrl);
            pm.SetContentType(item.ContentType);
            pm.SetSize(item.Size);
            pm.SetSortOrder(i);
            post.Media.Add(pm);
        }

        await _postRepository.Add(post);
        return post;
    }

    public async Task<Post> CreateAnnouncement(Guid authorMemberId, string content, IReadOnlyList<PostMediaItem> media)
    {
        return await CreatePost(null, authorMemberId, content, PostType.Text, media);
    }

    public async Task UpdateAnnouncement(Guid postId, string content, IReadOnlyList<PostMediaItem> media)
    {
        if (media.Count > 1)
            throw new InvalidOperationException("An announcement can have at most one image.");

        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) throw new InvalidOperationException("Post not found.");
        if (!post.IsAnnouncement) throw new InvalidOperationException("Post is not an announcement.");

        post.SetContent(content);
        post.Media.Clear();

        for (var i = 0; i < media.Count; i++)
        {
            var item = media[i];
            var pm = new PostMedia();
            pm.SetPost(post);
            pm.SetMediaUrl(item.DisplayUrl);
            pm.SetThumbnailUrl(item.ThumbnailUrl);
            pm.SetOriginalUrl(item.OriginalUrl);
            pm.SetContentType(item.ContentType);
            pm.SetSize(item.Size);
            pm.SetSortOrder(i);
            post.Media.Add(pm);
        }

        post.SetType(media.Count > 0 ? PostType.Photo : PostType.Text);
        await _postRepository.Update(post);
    }

    public async Task<Post> CreatePollPost(
        Guid groupId,
        Guid authorMemberId,
        string question,
        IReadOnlyList<string> options,
        bool allowMultipleAnswers)
    {
        var member = _memberRepository.FindById(authorMemberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var isMember = await _groupMemberRepository.IsMember(groupId, authorMemberId);
        if (!isMember) throw new InvalidOperationException("Not a member of this group.");

        var post = new Post();
        post.SetGroupId(groupId);
        post.SetAuthor(member);
        post.SetContent(string.Empty);
        post.SetType(PostType.Poll);

        await _postRepository.Add(post);

        var poll = Poll.Create(post, question, allowMultipleAnswers);
        for (var i = 0; i < options.Count; i++)
        {
            var option = PollOption.Create(poll, options[i], i);
            poll.Options.Add(option);
        }

        await _pollRepository.Add(poll);

        return post;
    }

    public async Task DeletePost(Guid postId)
    {
        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) throw new InvalidOperationException("Post not found.");
        post.SoftDelete();
        await _postRepository.Update(post);
    }

    public async Task PinPost(Guid postId, Guid groupId)
    {
        var currentPinned = await _postRepository.GetPinnedPost(groupId);
        if (currentPinned != null)
        {
            currentPinned.Unpin();
            await _postRepository.Update(currentPinned);
        }

        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) throw new InvalidOperationException("Post not found.");

        if (post.IsPinned)
            post.Unpin();
        else
            post.Pin();

        await _postRepository.Update(post);
    }

    public async Task<bool> ToggleLike(Guid postId, Guid memberId)
    {
        var existing = await _postRepository.FindReaction(postId, memberId);
        if (existing != null)
        {
            await _postRepository.RemoveReaction(existing);
            return false;
        }

        var member = _memberRepository.FindById(memberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) throw new InvalidOperationException("Post not found.");

        var reaction = new PostReaction();
        reaction.SetPost(post);
        reaction.SetMember(member);
        reaction.SetType(ReactionType.Like);
        reaction.SetCreatedAt(InstantHelper.GetLocalNow());

        await _postRepository.AddReaction(reaction);
        return true;
    }

    public async Task AddComment(Guid postId, Guid authorMemberId, string content)
    {
        var member = _memberRepository.FindById(authorMemberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) throw new InvalidOperationException("Post not found.");

        var comment = new Comment();
        comment.SetPost(post);
        comment.SetAuthor(member);
        comment.SetContent(content);

        await _commentRepository.Add(comment);
    }

    public async Task DeleteComment(Guid commentId)
    {
        var comment = await _commentRepository.FindById(commentId, asNoTracking: false);
        if (comment == null) throw new InvalidOperationException("Comment not found.");
        comment.SoftDelete();
        await _commentRepository.Update(comment);
    }

    public async Task RecordView(Guid postId, Guid memberId)
    {
        var hasView = await _postRepository.HasView(postId, memberId);
        if (hasView) return;

        var member = _memberRepository.FindById(memberId, asNoTracking: false);
        if (member == null) return;

        var post = await _postRepository.FindById(postId, asNoTracking: false);
        if (post == null) return;

        var view = new PostView();
        view.SetPost(post);
        view.SetMember(member);
        view.SetViewedAt(InstantHelper.GetLocalNow());

        post.IncrementViewCount();
        await _postRepository.AddView(view);
        await _postRepository.Update(post);
    }

    public async Task VoteOnPoll(Guid pollOptionId, Guid memberId)
    {
        var option = await _pollRepository.FindOptionById(pollOptionId);
        if (option == null) throw new InvalidOperationException("Poll option not found.");

        var member = _memberRepository.FindById(memberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var alreadyVotedThisOption = await _pollRepository.HasVoted(pollOptionId, memberId);
        if (alreadyVotedThisOption)
        {
            await _pollRepository.RemoveVote(pollOptionId, memberId);
            return;
        }

        if (!option.Poll.AllowMultipleAnswers)
        {
            await _pollRepository.RemoveVotesForPoll(option.PollId, memberId);
        }

        var vote = new PollVote();
        vote.SetPollOption(option);
        vote.SetMember(member);
        vote.SetCreatedAt(InstantHelper.GetLocalNow());

        await _pollRepository.AddVote(vote);
    }

    public async Task<PaginatedResult<Post>> GetGroupFeed(Guid groupId, int page)
    {
        const int pageSize = 20;
        var skip = (page - 1) * pageSize;
        var items = await _postRepository.GetByGroupId(groupId, skip, pageSize + 1);
        var hasMore = items.Count > pageSize;
        return new PaginatedResult<Post>(items.Take(pageSize).ToList(), hasMore);
    }

    public async Task<PaginatedResult<Post>> GetAnnouncements(int page)
    {
        const int pageSize = 10;
        var skip = (page - 1) * pageSize;
        var items = await _postRepository.GetAnnouncements(skip, pageSize + 1);
        var hasMore = items.Count > pageSize;
        return new PaginatedResult<Post>(items.Take(pageSize).ToList(), hasMore);
    }

    public async Task<Post?> GetPostById(Guid postId)
    {
        return await _postRepository.FindById(postId);
    }

    public async Task<PaginatedResult<Comment>> GetComments(Guid postId, int page)
    {
        const int pageSize = 10;
        var skip = (page - 1) * pageSize;
        var items = await _commentRepository.GetByPostId(postId, skip, pageSize + 1);
        var hasMore = items.Count > pageSize;
        return new PaginatedResult<Comment>(items.Take(pageSize).ToList(), hasMore);
    }
}
