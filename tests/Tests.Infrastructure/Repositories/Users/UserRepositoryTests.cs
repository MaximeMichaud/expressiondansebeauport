using Application.Exceptions.Users;
using Application.Extensions;
using Domain.Constants.User;
using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Repositories.Users;
using Infrastructure.Repositories.Users.Exceptions;
using Microsoft.AspNetCore.Identity;
using Tests.Common.Builders;
using Tests.Infrastructure.Fixtures;
using Tests.Infrastructure.TestCollections;

namespace Tests.Infrastructure.Repositories.Users;

[Collection(InfrastructureTestCollection.NAME)]
public class UserRepositoryTests
{
    private readonly PersistenceTestFixture _testFixture;
    private readonly UserBuilder _userBuilder;

    private readonly Mock<UserManager<User>> _userManager;

    private readonly IUserRepository _userRepository;

    public UserRepositoryTests(PersistenceTestFixture testFixture)
    {
        _testFixture = testFixture;
        _userBuilder = new UserBuilder();

        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _userRepository = new UserRepository(_userManager.Object);
    }

    [Theory]
    [InlineData("garneau@spektrummedia.com")]
    [InlineData("garneau@SPEKTRUMMEDIA.COM")]
    [InlineData("GARNEAU@spektrummedia.com")]
    public void GivenUserWithEmailExists_WhenUserWithEmailExists_ThenReturnTrue(string email)
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(email).Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var userWithEmailExists = _userRepository.UserWithEmailExists(email);

        // Assert
        userWithEmailExists.ShouldBeTrue();
    }

    [Fact]
    public void GivenUserWithEmailExistsButIsDeleted_WhenUserWithEmailExists_ThenReturnFalse()
    {
        // Arrange
        var email = _testFixture.GenerateEmail();
        var expectedUser = _userBuilder.WithEmail(email).AsDeleted().Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var userWithEmailExists = _userRepository.UserWithEmailExists(email);

        // Assert
        userWithEmailExists.ShouldBeFalse();
    }

    [Fact]
    public void GivenNoUserWithEmailExists_WhenUserWithEmailExists_ThenReturnFalse()
    {
        // Act
        var userWithEmailExists = _userRepository.UserWithEmailExists(_testFixture.GenerateEmail());

        // Assert
        userWithEmailExists.ShouldBeFalse();
    }

    [Fact]
    public void GivenUserWithIdExists_WhenFindById_ThenReturnMatchingUser()
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(_testFixture.GenerateEmail()).Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindById(expectedUser.Id);

        // Assert
        actualUser.ShouldBe(expectedUser);
    }

    [Fact]
    public void GivenUserWithIdExistsButIsDeleted_WhenFindById_ThenReturnNull()
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(_testFixture.GenerateEmail()).AsDeleted().Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindById(expectedUser.Id);

        // Assert
        actualUser.ShouldBeNull();
    }

    [Fact]
    public void GivenNoUserWithIdExists_WhenFindById_ThenReturnNull()
    {
        // Act
        var actualUser = _userRepository.FindById(Guid.NewGuid());

        // Assert
        actualUser.ShouldBeNull();
    }

    [Theory]
    [InlineData("garneau@spektrummedia.com")]
    [InlineData("garneau@SPEKTRUMMEDIA.COM")]
    [InlineData("GARNEAU@spektrummedia.com")]
    public void GivenUserWithUserNameExists_WhenFindByUserName_ThenReturnMatchingUser(string username)
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(username).Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByUserName(username);

        // Assert
        actualUser.ShouldBe(expectedUser);
    }

    [Fact]
    public void GivenUserWithUserNameExistsButIsDeleted_WhenFindByUserName_ThenReturnNull()
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(_testFixture.GenerateEmail()).AsDeleted().Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByUserName(expectedUser.UserName!);

        // Assert
        actualUser.ShouldBeNull();
    }

    [Fact]
    public void GivenNoUserWithUserNameExists_WhenFindByUserName_ThenReturnNull()
    {
        // Act
        var actualUser = _userRepository.FindByUserName(_testFixture.GenerateEmail());

        // Assert
        actualUser.ShouldBeNull();
    }

    [Fact]
    public void GivenIncludeDeletedIsFalseAndUserWithEmailExists_WhenFindByEmail_ThenReturnMatchingUser()
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(_testFixture.GenerateEmail()).Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByEmail(expectedUser.UserName!);

        // Assert
        actualUser.ShouldBe(expectedUser);
    }

    [Fact]
    public void GivenIncludeDeletedIsFalseAndUserWithEmailExistsButIsDeleted_WhenFindByEmail_ThenReturnNull()
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(_testFixture.GenerateEmail()).AsDeleted().Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByEmail(expectedUser.UserName!);

        // Assert
        actualUser.ShouldBeNull();
    }

    [Fact]
    public void GivenIncludeDeletedIsFalseAndNoUserWithEmailExists_WhenFindByEmail_ThenReturnNull()
    {
        // Act
        var actualUser = _userRepository.FindByEmail(_testFixture.GenerateEmail());

        // Assert
        actualUser.ShouldBeNull();
    }

    [Theory]
    [InlineData("garneau@spektrummedia.com")]
    [InlineData("GARNEAU@SPEKTRUMMEDIA.COM")]
    [InlineData("GARNEAU@spektrummedia.com")]
    public void GivenIncludeDeletedIsTrueAndUserWithEmailExists_WhenFindByEmail_ThenReturnMatchingUser(string email)
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(email).Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByEmail(email, true);

        // Assert
        actualUser.ShouldBe(expectedUser);
    }

    [Theory]
    [InlineData("garneau@spektrummedia.com")]
    [InlineData("GARNEAU@SPEKTRUMMEDIA.COM")]
    [InlineData("GARNEAU@spektrummedia.com")]
    public void GivenIncludeDeletedIsTrueAndUserWithEmailExistsButIsDeleted_WhenFindByEmail_ThenReturnMatchingUser(string email)
    {
        // Arrange
        var expectedUser = _userBuilder.WithEmail(email).AsDeleted().Build();
        _userManager.Setup(x => x.Users).Returns(expectedUser.IntoList().AsQueryable);

        // Act
        var actualUser = _userRepository.FindByEmail(email, true);

        // Assert
        actualUser.ShouldBe(expectedUser);
    }

    [Fact]
    public void GivenIncludeDeletedIsTrueAndNoUserWithEmailExists_WhenFindByEmail_ThenReturnNull()
    {
        // Act
        var actualUser = _userRepository.FindByEmail(_testFixture.GenerateEmail(), true);

        // Assert
        actualUser.ShouldBeNull();
    }

    [Fact]
    public async Task WhenSyncUser_ThenReturnCreatedOrUpdatedUser()
    {
        // Arrange
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToSync = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));
        GivenUserWithEmailExists(userToSync);

        // Act
        var createdUser = await _userRepository.SyncUser(userToSync);

        // Assert
        createdUser.ShouldNotBeNull();
        createdUser.Email.ShouldBe(email);
    }

    [Fact]
    public async Task GivenUserWithEmailDoesNotExist_WhenSyncUser_ThenCreateUser()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToSync = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));

        // Act
        await _userRepository.SyncUser(userToSync);

        // Assert
        _userManager.Verify(x => x.CreateAsync(It.IsAny<User>()));
    }

    [Fact]
    public async Task GivenUserWithEmailExists_WhenSyncUser_ThenUpdateUser()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable);
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());

        // Act
        await _userRepository.SyncUser(user);

        // Assert
        _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()));
    }

    [Fact]
    public async Task WhenCreateUser_ThenCreateUser()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToCreate = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));

        // Act
        await _userRepository.CreateUser(userToCreate);

        // Assert
        _userManager.Verify(x => x.CreateAsync(It.IsAny<User>()));
    }

    [Fact]
    public async Task WhenCreateUser_ThenAddUserToItsRole()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToCreate = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));

        // Act
        await _userRepository.CreateUser(userToCreate);

        // Assert
        _userManager.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), Roles.ADMINISTRATOR.IntoList()));
    }

    [Fact]
    public async Task WhenCreateUser_ThenReturnCreatedUser()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToCreate = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));
        GivenUserWithEmailExists(userToCreate);

        // Act
        var createdUser = await _userRepository.CreateUser(userToCreate);

        // Assert
        createdUser.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenCreateUser_ThenCreateWithGivenEmail()
    {
        // Arrange
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToCreate = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));
        GivenUserWithEmailExists(userToCreate);

        // Act
        var createdUser = await _userRepository.CreateUser(userToCreate);

        // Assert
        createdUser.Email.ShouldBe(email);
    }

    [Fact]
    public async Task WhenCreateUser_ThenCreateWithGivenRole()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userToCreate = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable));
        GivenUserWithEmailExists(userToCreate);

        // Act
        var createdUser = await _userRepository.CreateUser(userToCreate);

        // Assert
        createdUser.RoleNames.ShouldContain(Roles.ADMINISTRATOR);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GivenUserWithNullEmptyOrWhitespaceEmail_WhenCreateUser_ThenThrowCreateUserException(string? email)
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(email!).WithRole(role).Build();

        // Act & assert
        await Assert.ThrowsAsync<CreateUserException>(async () => await _userRepository.CreateUser(user));
    }

    [Fact]
    public async Task GivenUserWithNullRole_WhenCreateUser_ThenThrowCreateUserException()
    {
        // Arrange
        var user = _userBuilder.WithEmail(_testFixture.GenerateEmail()).Build();
        user.UserRoles.Clear();

        // Act & assert
        await Assert.ThrowsAsync<CreateUserException>(async () => await _userRepository.CreateUser(user));
    }

    [Fact]
    public async Task GivenUserWithSameEmailExists_WhenCreateUser_ThenThrowUserWithEmailAlreadyExistsException()
    {
        // Arrange
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(email).WithRole(role).Build();
        var otherUser = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(new List<User> { user, otherUser }.AsQueryable);

        // Act & assert
        await Assert.ThrowsAsync<UserWithEmailAlreadyExistsException>(
            async () => await _userRepository.CreateUser(user));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GivenUserWithNullEmptyOrWhitespaceEmail_WhenUpdateUser_ThenThrowCreateUserException(string? email)
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(email!).WithRole(role).Build();

        // Act & assert
        await Assert.ThrowsAsync<UpdateUserException>(async () => await _userRepository.UpdateUser(user));
    }

    [Fact]
    public async Task GivenUserWithNoRole_WhenUpdateUser_ThenThrowUpdateUserException()
    {
        // Arrange
        var user = _userBuilder.WithEmail(_testFixture.GenerateEmail()).Build();

        // Act & assert
        await Assert.ThrowsAsync<UpdateUserException>(async () => await _userRepository.UpdateUser(user));
    }

    [Fact]
    public async Task GivenUserWithIdThatDoesNotExistInDatabase_WhenUpdateUser_ThenThrowUserNotFoundException()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(_testFixture.GenerateEmail()).WithRole(role).Build();

        // Act & assert
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _userRepository.UpdateUser(user));
    }

    [Fact]
    public async Task GivenAnotherUserHasSameEmailAsRequestedEmail_WhenUpdateUser_ThenThrowUserWithEmailAlreadyExistsException()
    {
        // Arrange
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var userInDatabase = await _testFixture.GivenUserInDatabase();
        var user = _userBuilder.WithEmail(userInDatabase.Email).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(new List<User> {user, userInDatabase}.AsQueryable);

        // Act & assert
        await Assert.ThrowsAsync<UserWithEmailAlreadyExistsException>(
            async () => await _userRepository.UpdateUser(user));
    }

    [Fact]
    public async Task WhenUpdateUser_ThenUpdateUser()
    {
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable);
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());

        // Act
        await _userRepository.UpdateUser(user);

        // Act & assert
        _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()));
    }

     [Fact]
    public async Task WhenUpdateUser_ThenRemoveUserFromExistingRoles()
    {
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var roles = Roles.ADMINISTRATOR.IntoList();
        var user = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable);
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(roles);

        // Act
        await _userRepository.UpdateUser(user);

        // Act
        _userManager.Verify(x => x.RemoveFromRolesAsync(It.IsAny<User>(), roles));
    }

    [Fact]
    public async Task WhenUpdateUser_ThenAddUserToItsRole()
    {
        var email = _testFixture.GenerateEmail();
        var role = _testFixture.FindRoleWithName(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithEmail(email).WithRole(role).Build();
        _userManager.Setup(x => x.Users).Returns(user.IntoList().AsQueryable);
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());

        // Act
        await _userRepository.UpdateUser(user);

        // Act & assert
        _userManager.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), Roles.ADMINISTRATOR.IntoList()));
    }

    [Fact]
    public async Task GivenNoUserWithIdExists_WhenDeleteUser_ThenThrowUserNotFoundException()
    {
        // Arrange
        User? expectedUser = null;
        var user = _userBuilder.Build();
        _userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(expectedUser);

        // Act & assert
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _userRepository.DeleteUser(user));
    }

    [Fact]
    public async Task GivenUserWithIdExists_WhenDeleteUser_ThenSoftDeleteUser()
    {
        // Arrange
        var user = _userBuilder.Build();
        _userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        User? updatedUser = null;
        _userManager
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(userParam => updatedUser = userParam);

        // Act
        await _userRepository.DeleteUser(user);

        // Assert
        updatedUser.ShouldNotBeNull();
        updatedUser.IsActive().ShouldBeFalse();
    }

    [Fact]
    public async Task GivenUserWithIdExists_WhenDeleteUser_ThenDelegateUpdatingDeletedUserToUserManager()
    {
        // Arrange
        var user = _userBuilder.Build();
        _userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        await _userRepository.DeleteUser(user);

        // Assert
        _userManager.Verify(x => x.UpdateAsync(user));
    }

    [Fact]
    public async Task WhenGetResetPasswordTokenForUser_ThenDelegateResetPasswordTokenGenerationToUserManager()
    {
        // Arrange
        var user = _userBuilder.Build();

        // Act
        await _userRepository.GetResetPasswordTokenForUser(user);

        // Assert
        _userManager.Verify(x => x.GeneratePasswordResetTokenAsync(user));
    }

    [Fact]
    public async Task WhenCreateUserPassword_ThenDelegateAddingPasswordToUserToUserManager()
    {
        // Arrange
        const string anyPassword = "my password";
        var user = _userBuilder.Build();

        // Act
        await _userRepository.CreateUserPassword(user, anyPassword);

        // Assert
        _userManager.Verify(x => x.AddPasswordAsync(user, anyPassword));
    }

    [Fact]
    public async Task WhenUpdateUserPassword_ThenDelegateResettingUserPasswordToUserManager()
    {
        // Arrange
        const string anyToken = "my token";
        const string anyPasssword = "my new password";
        var user = _userBuilder.Build();

        // Act
        await _userRepository.ResetUserPassword(user, anyPasssword, anyToken);

        // Assert
        _userManager.Verify(x => x.ResetPasswordAsync(user, anyToken, anyPasssword));
    }

    private void GivenUserWithEmailExists(User user)
    {
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
    }
}