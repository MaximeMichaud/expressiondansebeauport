using Application.Mapping.Profiles;
using AutoMapper;
using Domain.Constants.User;
using Domain.Entities.Identity;
using Tests.Application.TestCollections;
using Tests.Common.Builders;
using Tests.Common.Fixtures;
using Tests.Common.Mapping;

namespace Tests.Application.Mapping.Profiles;

[Collection(ApplicationTestCollection.NAME)]
public class MemberMappingProfileTests
{
    private const string AnyFirstName = "john";
    private const string AnyLastName = "doe";
    private const string AnyEmail = "john.doe@gmail.com";
    private const string AnyPhoneNumber = "514-640-8920";
    private const int AnyPhoneExtension = 57;
    private const int AnyApartment = 3;
    private const string AnyStreet = "965, Hollywood Blvd";
    private const string AnyCity = "Hollywood";
    private const string AnyZipCode = "G7E 3L8";

    private readonly TestFixture _testFixture;

    private readonly UserBuilder _userBuilder;
    private readonly MemberBuilder _memberBuilder;

    private readonly IMapper _mapper;

    public MemberMappingProfileTests(TestFixture testFixture)
    {
        _testFixture = testFixture;

        _userBuilder = new UserBuilder();
        _memberBuilder = new MemberBuilder();

        _mapper = new MapperBuilder().WithProfile<MemberMappingProfile>().Build();
    }

    [Fact]
    public void GivenMember_WhenMap_ThenReturnUserMappedCorrectly()
    {
        // Arrange
        var memberUser = _userBuilder.WithRole(_testFixture.BuildRole(Roles.ADMINISTRATOR)).Build();
        var member = _memberBuilder
            .WithFirstName(AnyFirstName)
            .WithLastName(AnyLastName)
            .WithEmail(AnyEmail)
            .WithPhoneNumber(AnyPhoneNumber, AnyPhoneExtension)
            .WithApartment(AnyApartment)
            .WithStreet(AnyStreet)
            .WithCity(AnyCity)
            .WithZipCode(AnyZipCode)
            .WithUser(memberUser)
            .Build();

        // Act
        var user = _mapper.Map<User>(member);

        // Assert
        user.Id.ShouldBe(member.User.Id);
        user.Email.ShouldBe(AnyEmail.ToLower());
        user.UserName.ShouldBe(AnyEmail.ToLower());
        user.NormalizedEmail.ShouldBe(AnyEmail.Normalize());
        user.NormalizedUserName.ShouldBe(AnyEmail.Normalize());
        user.PhoneNumber.ShouldBe(AnyPhoneNumber);
        user.PhoneExtension.ShouldBe(AnyPhoneExtension);
        user.EmailConfirmed.ShouldBeTrue();
        user.PhoneNumberConfirmed.ShouldBeTrue();
        user.TwoFactorEnabled.ShouldBeTrue();
    }
}