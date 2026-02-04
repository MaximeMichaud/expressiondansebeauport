using Domain.Entities;
using Infrastructure.Repositories.Members;
using Tests.Infrastructure.Fixtures;
using Tests.Infrastructure.TestCollections;
namespace Tests.Infrastructure.Repositories.Members;

[Collection(InfrastructureTestCollection.NAME)]
public class MemberRepositoryTests
{
    private readonly PersistenceTestFixture _testFixture;

    private readonly MemberRepository _memberRepository;

    public MemberRepositoryTests(PersistenceTestFixture testFixture)
    {
        _testFixture = testFixture;
        _memberRepository = new MemberRepository(_testFixture.DbContext);
    }

    [Fact]
    public async Task GivenNoMemberWithUserIdExists_WhenFindByUserId_ThenReturnNull()
    {
        // Arrange
        var user = await _testFixture.GivenUserInDatabase();

        // Act
        var actualMember = _memberRepository.FindByUserId(user.Id);

        // Assert
        actualMember.ShouldBeNull();
    }

    [Fact]
    public async Task GivenMemberWithUserIdExists_WhenFindByUserId_ThenReturnMemberAssociatedWithUser()
    {
        // Arrange
        var expectedMember = await GivenDatabaseHasOrganizationMember();

        // Act
        var actualMember = _memberRepository.FindByUserId(expectedMember.User.Id);

        // Assert
        actualMember?.Id.ShouldBe(expectedMember.Id);
    }

    [Fact]
    public async Task WhenFindByUserEmail_ThenReturnMemberAssociatedWithUser()
    {
        // Arrange
        var expectedMember = await GivenDatabaseHasOrganizationMember();

        // Act
        var actualMember = _memberRepository.FindByUserEmail(expectedMember.User.Email!);

        // Assert
        actualMember?.Email.ShouldBe(expectedMember.Email);
    }

    private async Task<Member> GivenDatabaseHasOrganizationMember()
    {
        var user = await _testFixture.GivenUserInDatabase();
        var member = _testFixture.MemberBuilder.WithUser(user).Build();
        _testFixture.DbContext.Members.Add(member);
        await _testFixture.DbContext.SaveChangesAsync();
        return member;
    }
}