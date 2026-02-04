namespace Web.Extensions;

public static class HttpContextExtensions
{
    public static bool IsAuthenticated(this HttpContext httpContext)
    {
        var userIdentity =  httpContext.User.Identity;

        return userIdentity is { IsAuthenticated: true };
    }
}