using System.Globalization;
using Domain.Extensions;
using NodaTime;
using NodaTime.Text;

namespace Domain.Helpers;

public static class InstantHelper
{
    public static Instant GetLocalNow()
    {
        return DateTime.Now.ParseToInstant();
    }
    
    public static Instant ParseFromString(string dateString)
    {
        return InstantPattern.Create("uuuu-MM-dd", CultureInfo.InvariantCulture).Parse(dateString).Value;
    }
    
    public static Instant? ParseFromNullableString(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;
        return ParseFromString(dateString);
    }
}