using NodaTime;

namespace Domain.Extensions;

public static class InstantExtensions
{
    public static string FormatAsString(this Instant instant)
    {
        return instant.ToDateTimeUtc().FormatAsString();
    }

    public static string FormatAsStringWithTime(this Instant instant)
    {
        return instant.ToDateTimeUtc().FormatAsStringWithTime();
    }
}