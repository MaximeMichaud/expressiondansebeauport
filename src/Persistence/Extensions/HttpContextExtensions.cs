using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Persistence.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserEmail(this HttpContext? httpContext)
    {
        return httpContext?.User?.FindFirstValue(ClaimTypes.Email);
    }
}