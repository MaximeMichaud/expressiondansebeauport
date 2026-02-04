using Application.Extensions;

namespace Tests.Application.Extensions;

public class StringExtensionsTests
{
    private const string AnyValue = "hello";
    private const string Utf8Prefix = "=?UTF-8?B?";
    private const string StringWithIntValue = "34";
    private const int IntValueInsideString = 34;
    
    [Fact]
    public void WhenToRfc1342Base64_ThenReturnValueStartsWithUtf8()
    {
        var actual = AnyValue.ToRfc1342Base64();
        
        actual.ShouldStartWith(Utf8Prefix);
    }
    
    [Theory]
    [InlineData("", "")]
    [InlineData("12345", "MTIzNDU")]
    [InlineData(@"what_a-very+strange@email-address.com", "d2hhdF9hLXZlcnkrc3RyYW5nZUBlbWFpbC1hZGRyZXNzLmNvbQ")]
    [InlineData(@"@za^6$Nz3Ck//U%5\-++Hv3=69dbZ", "QHphXjYkTnozQ2svL1UlNVwtKytIdjM9NjlkYlo")]
    public void WhenBase64Encode_ThenReturnCorrectlyEncodedString(string clearText, string encoded)
    {
        // Arrange & act
        var result = clearText.Base64UrlEncode();
            
        // Assert
        result.ShouldBe(encoded);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("MTIzNDU", "12345")]
    [InlineData("d2hhdF9hLXZlcnkrc3RyYW5nZUBlbWFpbC1hZGRyZXNzLmNvbQ", @"what_a-very+strange@email-address.com")]
    [InlineData("QHphXjYkTnozQ2svL1UlNVwtKytIdjM9NjlkYlo", @"@za^6$Nz3Ck//U%5\-++Hv3=69dbZ")]
    public void WhenBase64Decode_ThenReturnCorrectlyDecodedString(string clearText, string encoded)
    {
        // Arrange & act
        var result = clearText.Base64UrlDecode();
            
        // Assert
        result.ShouldBe(encoded);
    }
    
    [Fact]
    public void GivenValidInt_WhenIntTryParseOrZero_ThenReturnCorrespondingInt()
    {
        // Arrange
        
        
        var actualValue = StringWithIntValue.IntTryParseOrZero();
            
        // Assert
        actualValue.ShouldBe(34);
    }
    
    [Fact]
    public void GivenStringWithValidIntInside_WhenIntTryParseOrZero_ThenReturnIntThatWasInString()
    {
        // Act
        var actualValue = StringWithIntValue.IntTryParseOrZero();
            
        // Assert
        actualValue.ShouldBe(IntValueInsideString);
    }
    
    [Fact]
    public void GivenStringWithNoIntInside_WhenIntTryParseOrZero_ThenReturnZero()
    {
        // Act
        var actualValue = AnyValue.IntTryParseOrZero();
            
        // Assert
        actualValue.ShouldBe(0);
    }
}