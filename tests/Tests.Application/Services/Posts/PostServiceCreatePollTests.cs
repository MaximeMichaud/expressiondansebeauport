using Application.Services.Posts;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;

namespace Tests.Application.Services.Posts;

public class PostServiceCreatePollTests
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
        var member = new Member();
        member.SetId(id);
        return member;
    }

    [Fact]
    public async Task GivenValidInput_WhenCreatePollPost_ThenCreatesPostAndPollWithOptions()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);
        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(true);
        _postRepository.Setup(r => r.Add(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _pollRepository.Setup(r => r.Add(It.IsAny<Poll>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var post = await service.CreatePollPost(
            groupId,
            memberId,
            "Quel horaire?",
            new[] { "Lundi 18h", "Mardi 19h", "Mercredi 20h" },
            allowMultipleAnswers: false);

        // Assert
        post.Type.ShouldBe(PostType.Poll);
        post.GroupId.ShouldBe(groupId);
        post.AuthorMemberId.ShouldBe(memberId);

        _postRepository.Verify(r => r.Add(It.Is<Post>(p =>
            p.Type == PostType.Poll && p.GroupId == groupId)), Times.Once);

        _pollRepository.Verify(r => r.Add(It.Is<Poll>(pl =>
            pl.Question == "Quel horaire?" &&
            pl.AllowMultipleAnswers == false &&
            pl.Options.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task GivenMemberNotInGroup_WhenCreatePollPost_ThenThrows()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var member = CreateMember(memberId);
        _memberRepository.Setup(r => r.FindById(memberId, false)).Returns(member);
        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(false);

        var service = CreateService();

        // Act + Assert
        await Should.ThrowAsync<InvalidOperationException>(() => service.CreatePollPost(
            groupId,
            memberId,
            "Question?",
            new[] { "A", "B" },
            allowMultipleAnswers: false));

        _postRepository.Verify(r => r.Add(It.IsAny<Post>()), Times.Never);
        _pollRepository.Verify(r => r.Add(It.IsAny<Poll>()), Times.Never);
    }
}
