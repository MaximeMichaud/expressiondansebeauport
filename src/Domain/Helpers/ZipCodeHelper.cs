using System.Text.RegularExpressions;

namespace Domain.Helpers;

public static partial class ZipCodeHelper
{
    [GeneratedRegex(@"^[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z]\s\d[ABCEGHJ-NPRSTV-Z]\d$")]
    private static partial Regex CanadianZipCodeRegex();

    public static bool HasCanadianZipCodeFormat(this string? zipCode)
    {
        return CanadianZipCodeRegex().IsMatch(zipCode ?? "");
    }
}