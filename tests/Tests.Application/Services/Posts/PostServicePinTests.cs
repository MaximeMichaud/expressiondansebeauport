using Application.Services.Posts;
using Domain.Entities;
using Domain.Repositories;

namespace Tests.Application.Services.Posts;

public class PostServicePinTests
{
    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<ICommentRepository> _commentRepository = new();
    private readonly Mock<IPollRepository> _pollRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<IGroupMemberRepository> _groupMemberRepository = new();

    private PostService CreateService() => new(
        _postRepository.Object,
        _commentRepository.Object,
        _pollRepository.Object,
        _memberRepository.Object,
        _groupMemberRepository.Object);

    private static Post CreatePost(Guid id, bool pinned = false)
    {
        var p = new Post();
        p.SetId(id);
        if (pinned) p.Pin();
        return p;
    }

    [Fact]
    public async Task GivenNoExistingPin_WhenPin_ThenReturnsIsPinnedTrueReplacedFalse()
    {
        var groupId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId);
        _postRepository.Setup(r => r.FindById(postId, false)).ReturnsAsync(post);
        _postRepository.Setup(r => r.GetPinnedPost(groupId)).ReturnsAsync((Post?)null);

        var result = await CreateService().PinPost(postId, groupId);

        result.IsPinned.ShouldBeTrue();
        result.ReplacedExisting.ShouldBeFalse();
        post.IsPinned.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenAlreadyPinned_WhenPin_ThenUnpinsAndReplacedFalse()
    {
        var groupId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId, pinned: true);
        _postRepository.Setup(r => r.FindById(postId, false)).ReturnsAsync(post);
        _postRepository.Setup(r => r.GetPinnedPost(groupId)).ReturnsAsync(post);

        var result = await CreateService().PinPost(postId, groupId);

        result.IsPinned.ShouldBeFalse();
        result.ReplacedExisting.ShouldBeFalse();
        post.IsPinned.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenAnotherPostPinned_WhenPin_ThenReplacesAndReplacedTrue()
    {
        var groupId = Guid.NewGuid();
        var newPostId = Guid.NewGuid();
        var oldPostId = Guid.NewGuid();
        var newPost = CreatePost(newPostId);
        var oldPost = CreatePost(oldPostId, pinned: true);
        _postRepository.Setup(r => r.FindById(newPostId, false)).ReturnsAsync(newPost);
        _postRepository.Setup(r => r.GetPinnedPost(groupId)).ReturnsAsync(oldPost);

        var result = await CreateService().PinPost(newPostId, groupId);

        result.IsPinned.ShouldBeTrue();
        result.ReplacedExisting.ShouldBeTrue();
        newPost.IsPinned.ShouldBeTrue();
        oldPost.IsPinned.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenPostNotFound_WhenPin_ThenThrows()
    {
        var groupId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _postRepository.Setup(r => r.FindById(postId, false)).ReturnsAsync((Post?)null);

        await Should.ThrowAsync<InvalidOperationException>(
            () => CreateService().PinPost(postId, groupId));
    }
}
