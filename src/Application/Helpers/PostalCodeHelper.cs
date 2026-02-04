using System.Text.RegularExpressions;

namespace Application.Helpers;

public static class PostalCodeHelper
{
    public static bool IsValidPostalCode(this string? postalCode)
    {
        string postalCodePattern = @"^[A-Za-z]\d[A-Za-z]\s?\d[A-Za-z]\d$";
        return Regex.IsMatch(postalCode ?? "", postalCodePattern);
    }
}