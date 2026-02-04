using Application.Exceptions.Members;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Users;
using Application.Services.Members;
using Application.Services.Members.Exceptions;
using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Repositories;
using Tests.Application.TestCollections;
using Tests.Common.Builders;
using Tests.Common.Fixtures;

namespace Tests.Application.Services.Members;

[Collection(ApplicationTestCollection.NAME)]
public class AuthenticatedMemberServiceTests
{
    private readonly TestFixture _testFixture;

    private readonly UserBuilder _userBuilder;
    private readonly MemberBuilder _memberBuilder;

    private readonly Mock<IMemberRepository> _memberRepository;
    private readonly Mock<IAuthenticatedUserService> _authenticatedUserService;

    private readonly AuthenticatedMemberService _authenticatedMemberService;

    public AuthenticatedMemberServiceTests(TestFixture testFixture)
    {
        _testFixture = testFixture;

        _userBuilder = new UserBuilder();
        _memberBuilder = new MemberBuilder();
        _memberRepository = new Mock<IMemberRepository>();
        _authenticatedUserService = new Mock<IAuthenticatedUserService>();

        _authenticatedMemberService = new AuthenticatedMemberService(_memberRepository.Object,
            _authenticatedUserService.Object);
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsNull_WhenGetAuthenticatedMember_ThenThrowUserNotFoundException()
    {
        // Act & assert
        Assert.Throws<UserNotFoundException>(() => _authenticatedMemberService.GetAuthenticatedMember());
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsUserWithoutRole_WhenGetAuthenticatedMember_ThenThrowGetAuthenticatedMemberException()
    {
        // Arrange
        var user = _userBuilder.Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        // Act & assert
        Assert.Throws<GetAuthenticatedMemberException>(() => _authenticatedMemberService.GetAuthenticatedMember());
    }

    [Fact]
    public void GivenMemberRepositoryDoesNotFindAnyMemberWithGivenUserId_WhenGetAuthenticatedMember_ThenThrowMemberNotFoundException()
    {
        // Arrange
        var role = _testFixture.BuildRole(Roles.ADMINISTRATOR);
        var user = _userBuilder.WithRole(role).Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        // Act & assert
        Assert.Throws<MemberNotFoundException>(() => _authenticatedMemberService.GetAuthenticatedMember());
    }

    [Fact]
    public void GivenAuthenticatedUserServiceReturnsUser_WhenGetAuthenticatedMember_ThenReturnMemberLinkedToAuthenticatedUser()
    {
        // Arrange
        var member = GivenAuthenticatedMemberWithUserIsFound();

        // Act
        var authenticatedMember = _authenticatedMemberService.GetAuthenticatedMember();

        // Assert
        authenticatedMember.ShouldNotBeNull();
        authenticatedMember.User.Id.ShouldBe(member.User.Id);
    }

    private void GivenAuthenticatedUserServiceReturnsUser(User user)
    {
        _authenticatedUserService.Setup(x => x.GetAuthenticatedUser()).Returns(user);
    }

    private Member GivenAuthenticatedMemberWithUserIsFound()
    {
        var user = _userBuilder.WithRole(_testFixture.BuildRole(Roles.ADMINISTRATOR)).Build();
        GivenAuthenticatedUserServiceReturnsUser(user);

        var member = _memberBuilder.WithUser(user).Build();
        _memberRepository.Setup(x => x.FindByUserId(user.Id, true)).Returns(member);
        return member;
    }
}