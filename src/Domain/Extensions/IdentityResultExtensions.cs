using Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Domain.Extensions;

public static class IdentityResultExtensions
{
    public static List<Error> GetErrors(this IdentityResult identityResult)
    {
        return identityResult.Errors.Select(x => new Error(x.Code, x.Description)).ToList();
    }
}