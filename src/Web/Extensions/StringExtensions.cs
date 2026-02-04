using System.Net.Mail;

namespace Web.Extensions;

public static class StringExtensions
{
    public static string SanitizeEmailAddress(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        
        try
        {
            var mailAddress = new MailAddress(value.Trim().Trim('.'));
            
            // Make sure there's at least one dot (.) after the @
            var parts = mailAddress.Address.Split("@");
            return parts.Length == 2 && parts[1].Contains('.')
                ? mailAddress.Address.ToLowerInvariant()
                : string.Empty;
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    public static bool IsValidIsbn(this string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 17)
            return false;
        return value.Split("-").Length == 5;
    }
}