using Domain.Extensions;

namespace Tests.Domain.Extensions;

public class DateTimeExtensionsTests
{
    private const int EXPECTED_YEAR = 2022;
    private const int EXPECTED_MONTH = 12;
    private const int EXPECTED_DAY = 10;
    private const int EXPECTED_HOUR = 8;
    private const int EXPECTED_MINUTE = 5;
    private const int EXPECTED_SECOND = 45;

    [Fact]
    public void GivenDateTime_WhenParseToInstant_ThenReturnInstant()
    {
        // Arrange
        var dateTime = new DateTime(EXPECTED_YEAR, EXPECTED_MONTH, EXPECTED_DAY, EXPECTED_HOUR, EXPECTED_MINUTE, EXPECTED_SECOND);

        // Act
        var instant = dateTime.ParseToInstant();

        // Assert
        instant.ToDateTimeUtc().Year.ShouldBe(EXPECTED_YEAR);
        instant.ToDateTimeUtc().Month.ShouldBe(EXPECTED_MONTH);
        instant.ToDateTimeUtc().Day.ShouldBe(EXPECTED_DAY);
        instant.ToDateTimeUtc().Hour.ShouldBe(EXPECTED_HOUR);
        instant.ToDateTimeUtc().Minute.ShouldBe(EXPECTED_MINUTE);
        instant.ToDateTimeUtc().Second.ShouldBe(EXPECTED_SECOND);
    }

    [Fact]
    public void WhenFormatAsString_ThenReturnDateFormattedWithYearMonthAndDaySeparedByHyphens()
    {
        // Arrange
        var dateTime = new DateTime(EXPECTED_YEAR, EXPECTED_MONTH, EXPECTED_DAY, EXPECTED_HOUR, EXPECTED_MINUTE, EXPECTED_SECOND);

        // Act
        var formattedDateTime = dateTime.FormatAsString();

        // Assert
        formattedDateTime.ShouldBe($"{EXPECTED_YEAR}-{EXPECTED_MONTH:00}-{EXPECTED_DAY:00}");
    }

    [Fact]
    public void WhenFormatAsStringWithTime_ThenReturnDateCorrectlyFormatted()
    {
        // Arrange
        var dateTime = new DateTime(EXPECTED_YEAR, EXPECTED_MONTH, EXPECTED_DAY, EXPECTED_HOUR, EXPECTED_MINUTE, EXPECTED_SECOND);

        // Act
        var formattedDateTime = dateTime.FormatAsStringWithTime();

        // Assert
        formattedDateTime.ShouldBe($"{EXPECTED_YEAR}-{EXPECTED_MONTH:00}-{EXPECTED_DAY:00} {EXPECTED_HOUR:00}:{EXPECTED_MINUTE:00}");
    }
}