using System.Text.RegularExpressions;

namespace Domain.Helpers;

public static partial class PhoneNumberHelper
{
    public const string EXTENSION_SEPARATOR = "poste";

    [GeneratedRegex(@"^(\d-)?\d{3}-\d{3}-\d{4}$")]
    private static partial Regex PhoneNumberRegex();

    public static bool HasValidPhoneNumberFormat(this string? number)
    {
        return PhoneNumberRegex().IsMatch(number ?? "");
    }

    public static string? AddExtensionToPhoneNumber(string? phoneNumber, int? extension)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || extension == null)
            return phoneNumber;
        return $"{phoneNumber} {EXTENSION_SEPARATOR} {extension}";
    }

    public static int? FindExtensionInPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var phoneNumberParts = phoneNumber.Split(EXTENSION_SEPARATOR);
        if (phoneNumberParts.Length < 2 || phoneNumberParts.Any(x => string.IsNullOrWhiteSpace(x.Trim())))
            return null;

        var stringExtension = phoneNumberParts[1].Trim();
        return int.TryParse(stringExtension, out var extension) ? extension : null;
    }

    public static string? RemoveExtensionFromPhoneNumber(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Split(EXTENSION_SEPARATOR)[0].Trim();
    }
}