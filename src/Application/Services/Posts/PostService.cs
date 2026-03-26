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

    public PostService(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IPollRepository pollRepository,
        IMemberRepository memberRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _pollRepository = pollRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Post> CreatePost(Guid? groupId, Guid authorMemberId, string content, PostType type)
    {
        var member = _memberRepository.FindById(authorMemberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var post = new Post();
        post.SetGroupId(groupId);
        post.SetAuthor(member);
        post.SetContent(content);
        post.SetType(type);

        await _postRepository.Add(post);
        return post;
    }

    public async Task<Post> CreateAnnouncement(Guid authorMemberId, string content)
    {
        return await CreatePost(null, authorMemberId, content, PostType.Text);
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

        var hasVoted = await _pollRepository.HasVotedOnPoll(option.PollId, memberId);
        if (hasVoted && !option.Poll.AllowMultipleAnswers)
            throw new InvalidOperationException("Already voted on this poll.");

        var alreadyVotedThisOption = await _pollRepository.HasVoted(pollOptionId, memberId);
        if (alreadyVotedThisOption) throw new InvalidOperationException("Already voted on this option.");

        var member = _memberRepository.FindById(memberId, asNoTracking: false);
        if (member == null) throw new InvalidOperationException("Member not found.");

        var vote = new PollVote();
        vote.SetPollOption(option);
        vote.SetMember(member);
        vote.SetCreatedAt(InstantHelper.GetLocalNow());

        await _pollRepository.AddVote(vote);
    }

    public async Task<List<Post>> GetGroupFeed(Guid groupId, int page)
    {
        var skip = (page - 1) * 20;
        return await _postRepository.GetByGroupId(groupId, skip, 20);
    }

    public async Task<List<Post>> GetAnnouncements(int page)
    {
        var skip = (page - 1) * 10;
        return await _postRepository.GetAnnouncements(skip, 10);
    }

    public async Task<Post?> GetPostById(Guid postId)
    {
        return await _postRepository.FindById(postId);
    }

    public async Task<List<Comment>> GetComments(Guid postId, int page)
    {
        var skip = (page - 1) * 10;
        return await _commentRepository.GetByPostId(postId, skip, 10);
    }
}
