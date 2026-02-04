using Domain.Entities;
using Infrastructure.Repositories.Admins;
using Tests.Infrastructure.Fixtures;
using Tests.Infrastructure.TestCollections;
namespace Tests.Infrastructure.Repositories.Admins;

[Collection(InfrastructureTestCollection.NAME)]
public class AdministratorRepositoryTests
{
    private readonly PersistenceTestFixture _testFixture;

    private readonly AdministratorRepository _administratorRepository;

    public AdministratorRepositoryTests(PersistenceTestFixture testFixture)
    {
        _testFixture = testFixture;
        _administratorRepository = new AdministratorRepository(_testFixture.DbContext);
    }

    [Fact]
    public async Task GivenNoAdministratorWithUserIdExists_WhenFindByUserId_ThenReturnNull()
    {
        // Arrange
        var user = await _testFixture.GivenUserInDatabase();

        // Act
        var actualMember = _administratorRepository.FindByUserId(user.Id);

        // Assert
        actualMember.ShouldBeNull();
    }

    [Fact]
    public async Task GivenAdministratorWithUserIdExists_WhenFindByUserId_ThenReturnAdministratorAssociatedWithUser()
    {
        // Arrange
        var expectedMember = await GivenDatabaseHasAnAdministrator();

        // Act
        var actualMember = _administratorRepository.FindByUserId(expectedMember.User.Id);

        // Assert
        actualMember?.Id.ShouldBe(expectedMember.Id);
    }

    private async Task<Administrator> GivenDatabaseHasAnAdministrator()
    {
        var user = await _testFixture.GivenUserInDatabase();
        var administrator = _testFixture.AdministratorBuilder.WithUser(user).Build();
        _testFixture.DbContext.Administrators.Add(administrator);
        await _testFixture.DbContext.SaveChangesAsync();
        return administrator;
    }
}