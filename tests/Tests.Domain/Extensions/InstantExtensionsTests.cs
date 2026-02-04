using Domain.Extensions;

namespace Tests.Domain.Extensions;

public class InstantExtensionsTests
{
    private const int EXPECTED_YEAR = 2022;
    private const int EXPECTED_MONTH = 12;
    private const int EXPECTED_DAY = 10;
    private const int EXPECTED_HOUR = 8;
    private const int EXPECTED_MINUTE = 5;
    private const int EXPECTED_SECOND = 45;

    [Fact]
    public void WhenFormatAsString_ThenReturnInstantFormattedWithYearMonthAndDaySeparedByHyphens()
    {
        // Arrange
        var dateTime = new DateTime(EXPECTED_YEAR, EXPECTED_MONTH, EXPECTED_DAY, EXPECTED_HOUR, EXPECTED_MINUTE, EXPECTED_SECOND);
        var instant = dateTime.ParseToInstant();

        // Act
        var formattedInstant = instant.FormatAsString();

        // Assert
        formattedInstant.ShouldBe($"{EXPECTED_YEAR}-{EXPECTED_MONTH:00}-{EXPECTED_DAY:00}");
    }

    [Fact]
    public void WhenFormatAsStringWithTime_ThenReturnInstantCorrectlyFormatted()
    {
        // Arrange
        var dateTime = new DateTime(EXPECTED_YEAR, EXPECTED_MONTH, EXPECTED_DAY, EXPECTED_HOUR, EXPECTED_MINUTE, EXPECTED_SECOND);
        var instant = dateTime.ParseToInstant();

        // Act
        var formattedInstant = instant.FormatAsStringWithTime();

        // Assert
        formattedInstant.ShouldBe($"{EXPECTED_YEAR}-{EXPECTED_MONTH:00}-{EXPECTED_DAY:00} {EXPECTED_HOUR:00}:{EXPECTED_MINUTE:00}");
    }
}