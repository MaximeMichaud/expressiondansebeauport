using Domain.Constants.User;
using Domain.Repositories;
using Infrastructure.Repositories.Users;
using Infrastructure.Repositories.Users.Exceptions;
using Tests.Infrastructure.Fixtures;
using Tests.Infrastructure.TestCollections;

namespace Tests.Infrastructure.Repositories.Users;

[Collection(InfrastructureTestCollection.NAME)]
public class RoleRepositoryTests
{
    private readonly IRoleRepository _roleRepository;

    public RoleRepositoryTests(PersistenceTestFixture testFixture)
    {
        _roleRepository = new RoleRepository(testFixture.RoleManager);
    }

    [Fact]
    public async Task GivenNoRoleWithNameExists_WhenFindByName_ThenThrowRoleNotFoundException()
    {
        // Arrange
        const string inexistantRoleName = "inexistant role";

        // Act & assert
        var exception = await Assert.ThrowsAsync<RoleNotFoundException>(
            async () => await _roleRepository.FindByName(inexistantRoleName));
        exception.Message.ShouldBe($"Could not find role with name {inexistantRoleName} in database.");
    }

    [Fact]
    public async Task GivenRoleWithNameExists_WhenFindByName_ThenReturnMatchingRole()
    {
        // Act
        var individualRole = await _roleRepository.FindByName(Roles.ADMINISTRATOR);

        // Assert
        individualRole.Name.ShouldBe(Roles.ADMINISTRATOR);
    }
}