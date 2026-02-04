using Domain.Helpers;

namespace Tests.Domain.Helpers;

public class PhoneNumberHelperTests
{
    private const string AnyPhoneNumber = "555-555-5555";
    private const int AnyPhoneExtension = 66;

    [Fact]
    public void GivenNullExtension_WhenAddExtensionToPhoneNumber_ThenReturnPhoneNumber()
    {
        // Act
        var phoneNumberWithExtension = PhoneNumberHelper.AddExtensionToPhoneNumber(AnyPhoneNumber, null);

        // Assert
        phoneNumberWithExtension.ShouldBe(AnyPhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespacePhoneNumber_WhenAddExtensionToPhoneNumber_ThenReturnPhoneNumber(string? phoneNumber)
    {
        // Act
        var phoneNumberWithExtension = PhoneNumberHelper.AddExtensionToPhoneNumber(phoneNumber, AnyPhoneExtension);

        // Assert
        phoneNumberWithExtension.ShouldBe(phoneNumber);
    }

    [Fact]
    public void WhenAddExtensionToPhoneNumber_ThenReturnPhoneNumberAndExtensionSeparatedWithSeparator()
    {
        // Act
        var phoneNumberWithExtension = PhoneNumberHelper.AddExtensionToPhoneNumber(AnyPhoneNumber, AnyPhoneExtension);

        // Assert
        phoneNumberWithExtension.ShouldBe($"{AnyPhoneNumber} {PhoneNumberHelper.EXTENSION_SEPARATOR} {AnyPhoneExtension}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespacePhoneNumber_WhenFindExtensionInPhoneNumber_ThenReturnNull(string? phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.FindExtensionInPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBeNull();
    }

    [Theory]
    [InlineData("555-555-5555")]
    [InlineData("1-888-495-3948 p.")]
    public void GivenPhoneNumberWithoutExtensionSeparator_WhenFindExtensionInPhoneNumber_ThenReturnNull(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.FindExtensionInPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBeNull();
    }

    [Theory]
    [InlineData("555-555-5555 poste ")]
    [InlineData("1-888-495-3948 poste")]
    public void GivenPhoneNumberWithoutExtensionNumber_WhenFindExtensionInPhoneNumber_ThenReturnNull(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.FindExtensionInPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBeNull();
    }

    [Theory]
    [InlineData("555-555-5555 poste 12")]
    [InlineData("1-888-495-3948  poste 12  ")]
    public void GivenPhoneNumberWithExtension_WhenFindExtensionInPhoneNumber_ThenReturnExtension(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.FindExtensionInPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBe(12);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GivenNullEmptyOrWhitespacePhoneNumber_WhenRemoveExtensionFromPhoneNumber_ThenReturnNull(string? phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.RemoveExtensionFromPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBeNull();
    }

    [Theory]
    [InlineData("555-555-5555")]
    [InlineData("1-888-495-3948 p.")]
    public void GivenPhoneNumberWithoutExtensionSeparator_WhenRemoveExtensionFromPhoneNumber_ThenReturnPhoneNumber(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.RemoveExtensionFromPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBe(phoneNumber);
    }

    [Theory]
    [InlineData("1-888-495-3948 poste ")]
    [InlineData("1-888-495-3948 poste")]
    public void GivenPhoneNumberWithoutExtensionNumber_WhenRemoveExtensionFromPhoneNumber_ThenReturnPhoneNumberWithoutSeparator(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.RemoveExtensionFromPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBe("1-888-495-3948");
    }

    [Theory]
    [InlineData("555-555-5555 poste 12")]
    [InlineData("555-555-5555  poste 14  ")]
    public void GivenPhoneNumberWithExtension_WhenRemoveExtensionFromPhoneNumber_ThenReturnExtension(string phoneNumber)
    {
        // Act
        var extension = PhoneNumberHelper.RemoveExtensionFromPhoneNumber(phoneNumber);

        // Assert
        extension.ShouldBe("555-555-5555");
    }
}