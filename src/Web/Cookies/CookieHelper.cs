
namespace Web.Cookies;

public static class CookieHelper
{
    public static string GetCookieValue(this HttpContext httpContext, string cookieName)
    {
        if (string.IsNullOrWhiteSpace(cookieName))
            return string.Empty;

        return (httpContext.Request.Cookies.ContainsKey(cookieName)
            ? httpContext.Request.Cookies[cookieName]
            : string.Empty)!;
    }

    public static void SetCookieValue(
        this HttpResponse response,
        string cookieName,
        string cookieValue,
        string domain,
        bool secure,
        bool httpOnly)
    {
        if (string.IsNullOrWhiteSpace(cookieName))
            return;

        var cookieOptions = new CookieOptions
        {
            Domain = domain,
            Path = "/",
            Secure = secure,
            HttpOnly = httpOnly,
            IsEssential = true,
            SameSite = SameSiteMode.Strict
        };

        response.Cookies.Append(cookieName, cookieValue, cookieOptions);
    }
}