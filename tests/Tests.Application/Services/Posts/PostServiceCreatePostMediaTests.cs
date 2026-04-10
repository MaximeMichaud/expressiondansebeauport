using Application.Services.Posts;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;

namespace Tests.Application.Services.Posts;

public class PostServiceCreatePostMediaTests
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

    private static Member CreateMember(Guid id)
    {
        var m = new Member();
        m.SetId(id);
        return m;
    }

    private static PostMediaItem MakeMedia(int n) => new PostMediaItem
    {
        DisplayUrl = $"/uploads/social/{n}.display.webp",
        ThumbnailUrl = $"/uploads/social/{n}.thumb.webp",
        OriginalUrl = $"/uploads/social/{n}.original.jpg",
        ContentType = "image/jpeg",
        Size = 12345
    };

    [Fact]
    public async Task GivenPhotoTypeAnd3Media_WhenCreatePost_ThenPostHas3PostMediaInOrder()
    {
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);
        _postRepository.Setup(r => r.Add(It.IsAny<Post>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var media = new[] { MakeMedia(1), MakeMedia(2), MakeMedia(3) };

        var post = await service.CreatePost(groupId, memberId, "test", PostType.Photo, media);

        post.Type.ShouldBe(PostType.Photo);
        post.Media.Count.ShouldBe(3);
        post.Media.ElementAt(0).MediaUrl.ShouldBe("/uploads/social/1.display.webp");
        post.Media.ElementAt(0).ThumbnailUrl.ShouldBe("/uploads/social/1.thumb.webp");
        post.Media.ElementAt(0).OriginalUrl.ShouldBe("/uploads/social/1.original.jpg");
        post.Media.ElementAt(0).SortOrder.ShouldBe(0);
        post.Media.ElementAt(2).SortOrder.ShouldBe(2);
    }

    [Fact]
    public async Task GivenTextTypeButMediaPresent_WhenCreatePost_ThenForcesPhotoType()
    {
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);
        _postRepository.Setup(r => r.Add(It.IsAny<Post>())).Returns(Task.CompletedTask);

        var service = CreateService();

        var post = await service.CreatePost(groupId, memberId, "hi", PostType.Text, new[] { MakeMedia(1) });

        post.Type.ShouldBe(PostType.Photo);
    }

    [Fact]
    public async Task GivenNoMedia_WhenCreatePost_ThenPostHasNoMedia()
    {
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);
        _postRepository.Setup(r => r.Add(It.IsAny<Post>())).Returns(Task.CompletedTask);

        var service = CreateService();

        var post = await service.CreatePost(groupId, memberId, "text only", PostType.Text, Array.Empty<PostMediaItem>());

        post.Type.ShouldBe(PostType.Text);
        post.Media.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenMoreThanTenMedia_WhenCreatePost_ThenThrows()
    {
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);

        var service = CreateService();
        var tooMany = Enumerable.Range(0, 11).Select(MakeMedia).ToArray();

        await Should.ThrowAsync<InvalidOperationException>(
            () => service.CreatePost(groupId, memberId, "x", PostType.Photo, tooMany));
    }
}
