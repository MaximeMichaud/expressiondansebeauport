using System.Text.RegularExpressions;

namespace Application.Helpers;

public static class EmailHelper
{
    public static bool IsValidEmail(this string? email)
    {
        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email?.Trim() ?? "", emailPattern);
    }
}