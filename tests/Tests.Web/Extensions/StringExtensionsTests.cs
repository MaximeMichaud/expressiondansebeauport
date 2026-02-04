using Web.Extensions;

namespace Tests.Web.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   ", "")]
    [InlineData("\n", "")]
    [InlineData("\r\n", "")]
    [InlineData("\t", "")]
    [InlineData(" john.smith@email.com ", "john.smith@email.com")]
    [InlineData("john.SMITH@eMAIL.com", "john.smith@email.com")]
    [InlineData("john.smith@email.com.", "john.smith@email.com")]
    [InlineData(".john.smith@email.com..", "john.smith@email.com")]
    [InlineData("someone@.com", "")]
    [InlineData("@email.com", "")]
    [InlineData("that.famous.person@email", "")]
    [InlineData("another.person", "")]
    public void SanitizeEmailAddress_ShouldDoItsJob(string input, string wanted)
    {
        // Act & assert
        input.SanitizeEmailAddress().ShouldBe(wanted);
    }
}