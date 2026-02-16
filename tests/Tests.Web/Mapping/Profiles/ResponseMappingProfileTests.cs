using AutoMapper;
using Domain.Common;
using Microsoft.AspNetCore.Identity;
using Tests.Common.Mapping;
using Tests.Web.TestCollections;
using Web.Constants;
using Domain.Common;
using Web.Mapping.Profiles;

namespace Tests.Web.Mapping.Profiles;

[Collection(WebTestCollection.NAME)]
public class ResponseMappingProfileTests
{
    private const string AnyErrorDescription = "Could not create user.";

    private readonly IMapper _mapper;

    public ResponseMappingProfileTests()
    {
        _mapper = new MapperBuilder().WithProfile<ResponseMappingProfile>().Build();
    }

    [Fact]
    public void GivenSuccessfulIdentityResult_WhenMap_ThenSucceededIsTrue()
    {
        // Arrange
        var identityResult = IdentityResult.Success;

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void GivenFailedIdentityResult_WhenMap_ThenSucceededContainsErrorWithIdentityCodeAsErrorType()
    {
        // Arrange
        var error = new IdentityError { Code = IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Errors.ShouldContain(x => x.ErrorType == error.Code);
    }

    [Fact]
    public void GivenFailedIdentityResult_WhenMap_ThenSucceededContainsErrorWithIdentityErrorDescriptionAsErrorMessage()
    {
        // Arrange
        var error = new IdentityError { Description = AnyErrorDescription };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var succeededOrNotResponse = _mapper.Map<SucceededOrNotResponse>(identityResult);

        // Assert
        succeededOrNotResponse.Errors.ShouldContain(x => x.ErrorMessage == AnyErrorDescription);
    }

    [Fact]
    public void GivenIdentityErrorWithCode_WhenMap_ThenErrorHasIdentityResultCodeAsErrorType()
    {
        // Arrange
        var identityError = new IdentityError { Code = IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD };

        // Act
        var error = _mapper.Map<Error>(identityError);

        // Assert
        error.ErrorType.ShouldBe(IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD);
    }

    [Fact]
    public void GivenIdentityErrorWithCode_WhenMap_ThenErrorHasIdentityResultDescriptionAsErrorMessage()
    {
        // Arrange
        var identityError = new IdentityError { Description = AnyErrorDescription };

        // Act
        var error = _mapper.Map<Error>(identityError);

        // Assert
        error.ErrorMessage.ShouldBe(AnyErrorDescription);
    }
}
