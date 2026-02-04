using System.IdentityModel.Tokens.Jwt;

namespace Domain.Extensions;

public static class StringExtensions
{
    public static string? CapitalizeFirstLetterOfEachWord(this string? value)
    {
        var lowercase = value?.ToLower();
        var result = lowercase?
            .CapitalizeFirstLetterOfEachWordSeparatedBy(" ")
            .CapitalizeFirstLetterOfEachWordSeparatedBy("-")
            .CapitalizeFirstLetterOfEachWordSeparatedBy(".");

        if (result == null)
            return result;

        var openingParenthesis = result.IndexOf('(');
        var closingParenthesis = result.IndexOf(')');
        if (openingParenthesis == -1 || closingParenthesis == -1)
            return result;

        var startOfSubstring = openingParenthesis + 1;
        var wordInsideParenthesis = result.Substring(startOfSubstring, closingParenthesis - startOfSubstring);
        var wordInsideParenthesisFormatted = wordInsideParenthesis
            .CapitalizeFirstLetterOfEachWordSeparatedBy(" ")
            .CapitalizeFirstLetterOfEachWordSeparatedBy("-")
            .CapitalizeFirstLetterOfEachWordSeparatedBy(".");
        return result.Replace(wordInsideParenthesis, wordInsideParenthesisFormatted);
    }

    private static string CapitalizeFirstLetterOfEachWordSeparatedBy(this string value, string separator)
    {
        var splitBySeparator = value.Split(separator);
        var firstLetterOfWordUpperCase = splitBySeparator.Select(x =>
        {
            if (x.Length == 0)
                return x;
            return x.Length == 1 ? x.ToUpper() : x[..1].ToUpper() + x[1..];
        });
        return string.Join(separator, firstLetterOfWordUpperCase);
    }

    public static JwtSecurityToken? ToJwtSecurityToken(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return new JwtSecurityToken(value.Replace("Bearer ", ""));
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string? GetUsernameFromJwtToken(this string value)
    {
        return value.ToJwtSecurityToken()?.Subject;
    }
}