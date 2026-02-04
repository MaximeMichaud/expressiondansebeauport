using Domain.Helpers;
using NodaTime.Text;

namespace Tests.Domain.Helpers;

public class InstantHelperTests
{
    private const string VALID_DATE = "2010-12-02";
    private const string INVALID_DATE = "20100-12-02";

    [Fact]
    public void GivenDateStringWithValidFormat_WhenParseFromString_ThenReturnInstantWithSameYearMonthAndDay()
    {
        // Act
        var instant = InstantHelper.ParseFromString(VALID_DATE);

        // Assert
        instant.ToDateTimeUtc().Day.ShouldBe(2);
        instant.ToDateTimeUtc().Month.ShouldBe(12);
        instant.ToDateTimeUtc().Year.ShouldBe(2010);
    }
    
    [Fact]
    public void GivenDateStringWithInvalidFormat_WhenParseFromString_ThenThrowUnparsableValueException()
    {
        // Act & assert
        Assert.Throws<UnparsableValueException>(() => InstantHelper.ParseFromString(INVALID_DATE));
    }
    
    
    [Fact]
    public void GivenNull_WhenParseFromNullableString_ThenReturnNull()
    {
        // Act
        var instant = InstantHelper.ParseFromNullableString(null);

        // Assert
        instant.ShouldBeNull();
    }
    
    [Fact]
    public void GivenDateStringWithValidFormat_WhenParseFromNullableString_ThenReturnInstantWithSameYearMonthAndDay()
    {
        // Act
        var instant = InstantHelper.ParseFromNullableString(VALID_DATE);

        // Assert
        instant.HasValue.ShouldBeTrue();
        instant!.Value.ToDateTimeUtc().Day.ShouldBe(2);
        instant.Value.ToDateTimeUtc().Month.ShouldBe(12);
        instant.Value.ToDateTimeUtc().Year.ShouldBe(2010);
    }
    
    [Fact]
    public void GivenDateStringWithInvalidFormat_WhenParseFromNullableString_ThenThrowUnparsableValueException()
    {
        // Act & assert
        Assert.Throws<UnparsableValueException>(() => InstantHelper.ParseFromNullableString(INVALID_DATE));
    }
}