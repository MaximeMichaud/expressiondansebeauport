using Application.Helpers;
using Application.Helpers.Exceptions;

namespace Tests.Application.Helpers;

public class CultureHelperTests
{
    private const string DefaultTwoLetterLang = "fr";
    private const string CanadianFrenchCultureName = "fr-CA";
    private const string UnsupportedTwoLetterLang = "de";

    [Fact]
    public void WhenGetDefaultCurrentCulture_ThenShouldReturnCanadianEnglishCultureInfo()
    {
        // Act
        var actual = CultureHelper.GetDefaultCulture();

        // Assert
        actual.Name.ShouldBe(CanadianFrenchCultureName);
    }

    [Fact]
    public void WhenGetCurrentCulture_ThenShouldReturnCurrentThreadCulture()
    {
        // Act
        var actual = CultureHelper.GetCurrentCulture();

        // Assert
        actual.ShouldBe(Thread.CurrentThread.CurrentCulture);
    }

    [Fact]
    public void WhenGetCurrentTwoLetterLang_ThenReturnTwoLetterISOLanguageName()
    {
        // Act
        var actual = CultureHelper.GetCurrentTwoLetterLang();

        // Assert
        actual.ShouldBe(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("fR")]
    [InlineData("Fr")]
    [InlineData("FR")]
    public void GivenTwoLetterCulture_WhenFormatTwoLetterCulture_ThenLowercaseTwoLetterCulture(string twoLetterCulture)
    {
        // Act
        var actual = CultureHelper.FormatTwoLetterCulture(twoLetterCulture);

        // Assert
        actual.ShouldBe("fr");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespaceValue_WhenFormatTwoLetterCulture_ThenReturnDefaultTwoLetterLang(string? twoLetterCulture)
    {
        // Act
        var actual = CultureHelper.FormatTwoLetterCulture(twoLetterCulture!);

        // Assert
        actual.ShouldBe(DefaultTwoLetterLang);
    }

    [Fact]
    public void GivenUnsupportedCulture_WhenFormatTwoLetterCulture_ThenThrowUnsupportedCultureException()
    {
        // Act & assert
        Assert.Throws<UnsupportedCultureException>(() =>
            CultureHelper.FormatTwoLetterCulture(UnsupportedTwoLetterLang));
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("en")]
    public void GivenSupportedTwoLetterCulture_WhenConvertTwoLetterIsoToCultureInfo_ThenMatchingCultureInfo(string twoLetterLang)
    {
        // Act
        var actual = CultureHelper.ConvertTwoLetterIsoToCultureInfo(twoLetterLang);

        // Assert
        actual.TwoLetterISOLanguageName.ShouldBe(twoLetterLang);
    }
}