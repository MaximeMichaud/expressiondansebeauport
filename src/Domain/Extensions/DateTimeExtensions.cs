using System.Globalization;
using NodaTime;

namespace Domain.Extensions;

public static class DateTimeExtensions
{
    public static Instant ParseToInstant(this DateTime dateTime)
    {
        return Instant.FromUtc(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }

    public static string FormatAsString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string FormatAsStringWithTime(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }
}