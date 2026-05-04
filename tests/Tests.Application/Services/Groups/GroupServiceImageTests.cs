using Application.Services.Groups;
using Domain.Entities;
using Domain.Repositories;

namespace Tests.Application.Services.Groups;

public class GroupServiceImageTests
{
    private readonly Mock<IGroupRepository> _groupRepository = new();
    private readonly Mock<IGroupMemberRepository> _groupMemberRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();

    private GroupService CreateService() => new(
        _groupRepository.Object,
        _groupMemberRepository.Object,
        _memberRepository.Object);

    private static Group CreateGroup(Guid id, string? imageUrl = null)
    {
        var group = new Group();
        group.SetId(id);
        group.SetName("Test");
        group.SetSeason("Hiver 2026");
        group.SetInviteCode("ABC123");
        if (imageUrl != null) group.SetImageUrl(imageUrl);
        return group;
    }

    [Fact]
    public async Task GivenMemberInGroup_WhenSetImageForMember_ThenGroupImageUpdated()
    {
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var group = CreateGroup(groupId);

        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(true);
        _groupRepository.Setup(r => r.FindById(groupId, false)).ReturnsAsync(group);
        _groupRepository.Setup(r => r.Update(It.IsAny<Group>())).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.SetImageForMember(groupId, memberId, "/uploads/abc.jpg");

        group.ImageUrl.ShouldBe("/uploads/abc.jpg");
        _groupRepository.Verify(r => r.Update(group), Times.Once);
    }

    [Fact]
    public async Task GivenMemberNotInGroup_WhenSetImageForMember_ThenThrowsAndDoesNotSave()
    {
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(false);

        var service = CreateService();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            service.SetImageForMember(groupId, memberId, "/uploads/abc.jpg"));

        _groupRepository.Verify(r => r.Update(It.IsAny<Group>()), Times.Never);
    }

    [Fact]
    public async Task GivenMemberInGroup_WhenClearImageForMember_ThenGroupImageNulled()
    {
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var group = CreateGroup(groupId, "/uploads/old.jpg");

        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(true);
        _groupRepository.Setup(r => r.FindById(groupId, false)).ReturnsAsync(group);
        _groupRepository.Setup(r => r.Update(It.IsAny<Group>())).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.ClearImageForMember(groupId, memberId);

        group.ImageUrl.ShouldBeNull();
        _groupRepository.Verify(r => r.Update(group), Times.Once);
    }

    [Fact]
    public async Task GivenMemberNotInGroup_WhenClearImageForMember_ThenThrowsAndDoesNotSave()
    {
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        _groupMemberRepository.Setup(r => r.IsMember(groupId, memberId)).ReturnsAsync(false);

        var service = CreateService();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            service.ClearImageForMember(groupId, memberId));

        _groupRepository.Verify(r => r.Update(It.IsAny<Group>()), Times.Never);
    }
}
