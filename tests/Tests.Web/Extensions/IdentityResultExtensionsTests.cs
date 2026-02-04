using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Web.Constants;
using Web.Extensions;

namespace Tests.Web.Extensions;

public class IdentityResultExtensionsTests
{
    private readonly Mock<IStringLocalizer> _localizer;
    
    public IdentityResultExtensionsTests()
    {
        _localizer = new Mock<IStringLocalizer>();
    }
    
    [Fact]
    public void GivenIdentityResultHasExceptionOfTypeUserAlreadyHasPassword_WhenGetErrorMessageForIdentityResultException_ThenReturnUserAlreadyHasPasswordMessage()
    {
        // Arrange
        const string errorMessageKey = "UserAlreadyHasPassword";
        const string errorMessage = "User already has a password.";
        _localizer.Setup(x => x[errorMessageKey]).Returns(new LocalizedString(errorMessageKey, errorMessage));
        var error = new IdentityError { Code = IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var message = identityResult.GetErrorMessageForIdentityResultException(_localizer.Object);
        
        message.ShouldBe(errorMessage);
    }
    
    [Fact]
    public void GivenIdentityResultWithoutErrorCodes_WhenGetErrorMessageForIdentityResultException_ThenReturnCouldNotChangePasswordMessage()
    {
        // Arrange
        const string errorMessageKey = "CouldNotChangePassword";
        const string errorMessage = "Could not change password";
        _localizer.Setup(x => x[errorMessageKey]).Returns(new LocalizedString(errorMessageKey, errorMessage));
        var identityResult = new IdentityResult();
        
        // Act
        var message = identityResult.GetErrorMessageForIdentityResultException(_localizer.Object);
        
        message.ShouldBe(errorMessage);
    }
    
    [Fact]
    public void GivenHasErrorOtherThanUserAlreadyHasPassword_WhenGetErrorMessageForIdentityResultException_ThenReturnMessageThatIsNotUserAlreadyHasPassword()
    {
        // Arrange
        const string errorMessageKey = "UserAlreadyHasPassword";
        const string errorMessage = "User already has a password.";
        _localizer.Setup(x => x[errorMessageKey]).Returns(new LocalizedString(errorMessageKey, errorMessage));
        var error = new IdentityError { Code = "OTHER_CODE" };
        var identityResult = IdentityResult.Failed(error);

        // Act
        var message = identityResult.GetErrorMessageForIdentityResultException(_localizer.Object);
        
        message.ShouldNotBe(errorMessage);
    }
}