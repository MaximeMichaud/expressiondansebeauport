using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts a string to base64 and returns an RFC 1342 compliant string. This ensures non ASCII characters will
    /// be correctly interpreted by mail clients.
    /// </summary>
    public static string ToRfc1342Base64(this string value)
    {
        return $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";
    }

    public static string Base64UrlEncode(this string value)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(value));
    }

    public static string Base64UrlDecode(this string value)
    {
        return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value));
    }

    public static int IntTryParseOrZero(this string? value)
    {
        return int.TryParse(value, out var number) ? number : 0;
    }
}
