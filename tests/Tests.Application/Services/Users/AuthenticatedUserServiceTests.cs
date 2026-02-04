using Application.Interfaces.Services;
using Application.Services.Users;
using Application.Services.Users.Exceptions;
using Domain.Entities.Identity;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Tests.Common.Builders;

namespace Tests.Application.Services.Users;

public class AuthenticatedUserServiceTests
{
    private const string AnyPassword = "Qwerty123!";
    private const string OtherPassword = "Qwerty1234!";
    private const string AnyEmail = "john.jane.doe@gmail.com";
    private const string OtherEmail = "john.jane.doe1@gmail.com";

    private readonly UserBuilder _userBuilder;

    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IHttpContextUserService> _httpContextUserService;

    private readonly AuthenticatedUserService _authenticatedUserService;

    public AuthenticatedUserServiceTests()
    {
        _userBuilder = new UserBuilder();

        _userRepository = new Mock<IUserRepository>();
        _httpContextUserService = new Mock<IHttpContextUserService>();

        _authenticatedUserService = new AuthenticatedUserService(_userRepository.Object,
            _httpContextUserService.Object);
    }

    [Fact]
    public void WhenGetAuthenticatedUser_ThenReturnUserWithSameEmailAsAuthenticatedUser()
    {
        // Arrange
        var user = GivenAuthenticatedUserExists();

        // Act
        var authenticatedUser = _authenticatedUserService.GetAuthenticatedUser();

        // Assert
        authenticatedUser!.Email.ShouldBe(user.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GivenNullEmptyOrWhitespaceCurrentPassword_WhenChangeUserPassword_ThenThrowChangeAuthenticatedUserPasswordException(string? currentUserPassword)
    {
        // Act & assert
        await Assert.ThrowsAsync<ChangeAuthenticatedUserPasswordException>(
            async () => await _authenticatedUserService.ChangeUserPassword(currentUserPassword!, AnyPassword));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GivenNullEmptyOrWhitespaceNewPassword_WhenChangeUserPassword_ThenThrowChangeAuthenticatedUserPasswordException(string? newPassword)
    {
        // Act & assert
        await Assert.ThrowsAsync<ChangeAuthenticatedUserPasswordException>(
            async () => await _authenticatedUserService.ChangeUserPassword(AnyPassword, newPassword!));
    }

    [Fact]
    public async Task GivenNoUserWithSpecifiedEmailExists_WhenChangeUserPassword_ThenThrowChangeAuthenticatedUserPasswordException()
    {
        // Arrange
        _httpContextUserService.Setup(x => x.Username).Returns(AnyEmail);

        // Act & assert
        await Assert.ThrowsAsync<ChangeAuthenticatedUserPasswordException>(
            async () => await _authenticatedUserService.ChangeUserPassword(AnyPassword, OtherPassword));
    }

    [Fact]
    public async Task GivenIncorrectCurrentPassword_WhenChangeUserPassword_ThenReturnUnsuccessfulIdentityResult()
    {
        // Arrange
        var user = GivenAuthenticatedUserExists();
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Incorrect password" });
        _userRepository.Setup(x => x.UpdateUserPassword(user, OtherPassword, OtherPassword))
            .ReturnsAsync(failedResult);

        // Act
        var identityResult = await _authenticatedUserService.ChangeUserPassword(OtherPassword, OtherPassword);

        // Assert
        identityResult.Succeeded.ShouldBeFalse();
        identityResult.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GivenUserWithGivenEmailExistsAndCurrentPasswordIsCorrect_WhenChangeUserPassword_ThenReturnSuccessfulIdentityResult()
    {
        // Arrange
        var user = GivenAuthenticatedUserExists();
        _userRepository.Setup(x => x.UpdateUserPassword(user, AnyPassword, OtherPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var identityResult = await _authenticatedUserService.ChangeUserPassword(AnyPassword, OtherPassword);

        // Assert
        identityResult.Succeeded.ShouldBeTrue();
        identityResult.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenNoUserIsAuthenticated_WhenChangeUserEmail_ThenThrowChangeAuthenticatedUserEmailException()
    {
        // Act & assert
        await Assert.ThrowsAsync<ChangeAuthenticatedUserEmailException>(
            async () => await _authenticatedUserService.ChangeUserEmail(AnyEmail));
    }

    [Fact]
    public async Task GivenUserIsAuthenticated_WhenChangeUserEmail_ThenReturnSuccessfulIdentityResult()
    {
        // Arrange
        var user = GivenAuthenticatedUserExists();
        _userRepository.Setup(x => x.UpdateUserEmail(user, OtherEmail))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var identityResult = await _authenticatedUserService.ChangeUserEmail(OtherEmail);

        // Assert
        identityResult.Succeeded.ShouldBeTrue();
        identityResult.Errors.ShouldBeEmpty();
    }

    private User GivenAuthenticatedUserExists()
    {
        var user = _userBuilder.Build();
        _httpContextUserService.Setup(x => x.Username).Returns(user.Email);
        _userRepository.Setup(x => x.FindByEmail(user.Email!, false)).Returns(user);
        return user;
    }
}