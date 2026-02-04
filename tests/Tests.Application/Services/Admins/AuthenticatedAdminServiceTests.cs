using Application.Exceptions.Admins;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Users;
using Application.Services.Admins;
using Application.Services.Admins.Exceptions;
using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Repositories;
using Tests.Application.TestCollections;
using Tests.Common.Builders;
using Tests.Common.Fixtures;

namespace Tests.Application.Services.Admins;

[Collection(ApplicationTestCollection.NAME)]
public class AuthenticatedAdminServiceTests
{
    private readonly TestFixture _testFixture;

    private readonly UserBuilder _userBuilder;
    private readonly AdministratorBuilder _administratorBuilder;

    private readonly Mock<IAdministratorRepository> _administratorRepository;
    private readonly Mock<IAuthenticatedUserService> _authenticatedUserService;

    private readonly AuthenticatedAdminService _authenticatedAdminService;

    public AuthenticatedAdminServiceTests(TestFixture testFixture)
    {
        _testFixture = testFixture;

        _userBuilder = new UserBuilder();
        _administratorBuilder = new AdministratorBuilder();
        _administratorRepository = new Mock<IAdministratorRepository>();
        _authenticatedUserService = new Mock<IAuthenticatedUserService>();

        _authenticatedAdminService = new AuthenticatedAdminService(_administratorRepository.Object,
            _authenticatedUserService.Object);
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsNull_WhenGetAuthenticatedAdmin_ThenThrowUserNotFoundException()
    {
        // Act & assert
        Assert.Throws<UserNotFoundException>(() => _authenticatedAdminService.GetAuthenticatedAdmin());
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsUserWithoutRole_WhenGetAuthenticatedAdmin_ThenThrowGetAuthenticatedAdminException()
    {
        // Arrange
        var user = _userBuilder.Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        // Act & assert
        Assert.Throws<GetAuthenticatedAdminException>(() => _authenticatedAdminService.GetAuthenticatedAdmin());
    }

    [Fact]
    public void GivenAdminRepositoryDoesNotFindAnyAdminWithGivenUserId_WhenGetAuthenticatedAdmin_ThenThrowAdministratorNotFoundException()
    {
        // Arrange
        var role = _testFixture.BuildRole(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithRole(role).Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        // Act & assert
        Assert.Throws<AdministratorNotFoundException>(() => _authenticatedAdminService.GetAuthenticatedAdmin());
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsUser_WhenGetAuthenticatedAdmin_ThenReturnAdminLinkedToAuthenticatedUser()
    {
        // Arrange
        var member = GivenAuthenticatedAdminWithUserIsFound();

        // Act
        var authenticatedAdmin = _authenticatedAdminService.GetAuthenticatedAdmin();

        // Assert
        authenticatedAdmin.ShouldNotBeNull();
        authenticatedAdmin.User.Id.ShouldBe(member.User.Id);
    }

    private void GivenAuthenticatedUserServiceReturnsUser(User user)
    {
        _authenticatedUserService.Setup(x => x.GetAuthenticatedUser()).Returns(user);
    }

    private Administrator GivenAuthenticatedAdminWithUserIsFound()
    {
        var user = _userBuilder.WithRole(_testFixture.BuildRole(Roles.ADMINISTRATOR)).Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        var member = _administratorBuilder.WithUser(user).Build();
        _administratorRepository.Setup(x => x.FindByUserId(user.Id, true)).Returns(member);
        return member;
    }
}