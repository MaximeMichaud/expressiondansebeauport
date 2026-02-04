using NodaTime;

namespace Application.Helpers;

public static class InstantHelper
{
    public static bool IsEmptyOrValid(this Instant? instant)
    {
        return !instant.HasValue ||
               instant.Value.ToDateTimeUtc() != DateTime.MinValue.ToUniversalTime();
    }
}