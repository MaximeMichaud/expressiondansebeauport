using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects;

public class PhoneNumberTests
{
    private const string ANY_NUMBER = "514-640-8920";
    private const string NUMBER_WITH_WHITE_SPACE = "  514-640-8920   ";
    private const int ANY_EXTENSION = 57;

    [Fact]
    public void WhenToString_ThenReturnNumberAndExtensionSeparatedByExtensionSeparator()
    {
        // Arrange
        var phoneNumber = new PhoneNumber(ANY_NUMBER, ANY_EXTENSION);

        // Act
        var formattedPhoneNumber = phoneNumber.ToString();

        // Assert
        formattedPhoneNumber.ShouldBe($"{ANY_NUMBER} {PhoneNumber.EXTENSION_SEPARATOR} {ANY_EXTENSION}");
    }

    [Fact]
    public void WhenToShortString_ThenReturnNumberAndExtensionSeparatedByComma()
    {
        // Arrange
        var phoneNumber = new PhoneNumber(ANY_NUMBER, ANY_EXTENSION);

        // Act
        var formattedPhoneNumber = phoneNumber.ToShortString();

        // Assert
        formattedPhoneNumber.ShouldBe($"{ANY_NUMBER}, {ANY_EXTENSION}");
    }

    [Fact]
    public void WhenSanitizeForSaving_ThenTrimNumber()
    {
        // Arrange
        var phoneNumber = new PhoneNumber(NUMBER_WITH_WHITE_SPACE, ANY_EXTENSION);

        // Act
        phoneNumber.SanitizeForSaving();

        // Assert
        phoneNumber.Number.ShouldBe(ANY_NUMBER);
    }
}