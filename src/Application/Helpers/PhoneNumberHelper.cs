using System.Text.RegularExpressions;

namespace Application.Helpers;

public static class PhoneNumberHelper
{
    public static bool IsValidPhoneNumber(this string? number)
    {
        const string phoneNumberPattern = @"^[0-9]{3}-[0-9]{3}-[0-9]{4}$";
        return Regex.IsMatch(number ?? "", phoneNumberPattern);
    }
}