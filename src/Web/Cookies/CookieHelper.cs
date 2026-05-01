
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
        bool httpOnly,
        TimeSpan? maxAge = null)
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
            SameSite = SameSiteMode.Lax,
            Expires = maxAge.HasValue ? DateTimeOffset.UtcNow.Add(maxAge.Value) : null
        };

        response.Cookies.Append(cookieName, cookieValue, cookieOptions);
    }

    public static void DeleteAuthCookie(
        this HttpResponse response,
        string cookieName,
        string domain,
        bool secure,
        bool httpOnly)
    {
        var cookieOptions = new CookieOptions
        {
            Domain = domain,
            Path = "/",
            Secure = secure,
            HttpOnly = httpOnly,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        response.Cookies.Append(cookieName, string.Empty, cookieOptions);
    }

    public static void IssueAuthCookies(
        this HttpResponse response,
        string accessToken,
        string refreshToken,
        string domain,
        bool secure,
        TimeSpan maxAge)
    {
        response.SetCookieValue(CookieName.ACCESS, accessToken, domain, secure, httpOnly: true, maxAge);
        response.SetCookieValue(CookieName.REFRESH, refreshToken, domain, secure, httpOnly: true, maxAge);
        response.SetCookieValue(CookieName.HAS_SESSION, "1", domain, secure, httpOnly: false, maxAge);
    }

    public static void ClearAuthCookies(
        this HttpResponse response,
        string domain,
        bool secure)
    {
        response.DeleteAuthCookie(CookieName.ACCESS, domain, secure, httpOnly: true);
        response.DeleteAuthCookie(CookieName.REFRESH, domain, secure, httpOnly: true);
        response.DeleteAuthCookie(CookieName.HAS_SESSION, domain, secure, httpOnly: false);
    }
}
