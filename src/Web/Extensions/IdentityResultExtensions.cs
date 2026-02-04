using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Web.Constants;

namespace Web.Extensions;

public static class IdentityResultExtensions
{
    public static bool HasExceptionOfType(this IdentityResult identityResult, string exceptionType)
    {
        return identityResult.Errors.Any(x => x.Code == exceptionType);
    }

    public static List<string> GetErrorCodes(this IdentityResult identityResult)
    {
        return identityResult.Errors.Select(x => x.Code).ToList();
    }
    
    public static string GetErrorMessageForIdentityResultException(this IdentityResult identityResult, IStringLocalizer localizer)
    {
        if (identityResult.HasExceptionOfType(IdentityResultExceptions.USER_ALREADY_HAS_PASSWORD))
            return localizer["UserAlreadyHasPassword"];

        var errorCodes = identityResult.GetErrorCodes();
        if (!errorCodes.Any())
            return localizer["CouldNotChangePassword"];
        
        var errorListItems = errorCodes.Select(x => $"<li>{localizer[x]}</li>");
        var errorList = $"<ul>{string.Join(string.Empty, errorListItems)}</ul>";
        return $"<div><p>{localizer["PasswordRequirements"]}</p>{errorList}</div>";
    }
}