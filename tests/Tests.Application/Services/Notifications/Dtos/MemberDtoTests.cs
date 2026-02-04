using Application.Services.Notifications.Dtos;

namespace Tests.Application.Services.Notifications.Dtos;

public class MemberDtoTests
{
    private const string AnyFirstName = "john";
    private const string AnyLastName = "doe";
    private const string AnyEmail = "john.doe@gmail.com";
    private const string AnyPhoneNumber = "514-640-8920";
    private const int AnyApartment = 2;
    private const string AnyStreet = "965, Hollywood Blvd";
    private const string AnyCity = "Hollywood";
    private const string AnyZipCode = "G7E 3L8";
    
    [Fact]
    public void GivenAnyFirstName_WhenNewMemberDto_ThenFirstNameShouldBeSameAsGivenFirstName()
    {
        // Act
        var memberDto = new PersonDto { FirstName = AnyFirstName };

        // Assert
        memberDto.FirstName.ShouldBe(AnyFirstName);
    }
    
    [Fact]
    public void GivenAnyLastName_WhenNewMemberDto_ThenLastNameShouldBeSameAsGivenLastName()
    {
        // Act
        var memberDto = new PersonDto { LastName = AnyLastName };

        // Assert
        memberDto.LastName.ShouldBe(AnyLastName);
    }
    
    [Fact]
    public void GivenAnyEmail_WhenNewMemberDto_ThenEmailAddressShouldBeSameAsGivenEmailAddress()
    {
        // Act
        var memberDto = new PersonDto { Email = AnyEmail };

        // Assert
        memberDto.Email.ShouldBe(AnyEmail);
    }
    
    [Fact]
    public void GivenAnyPhoneNumber_WhenNewMemberDto_ThenPhoneNumberShouldBeSameAsGivenPhoneNumber()
    {
        // Act
        var memberDto = new PersonDto { PhoneNumber = AnyPhoneNumber };

        // Assert
        memberDto.PhoneNumber.ShouldBe(AnyPhoneNumber);
    }
    
    [Fact]
    public void GivenAnyApartment_WhenNewMemberDto_ThenApartmentShouldBeSameAsGivenApartment()
    {
        // Act
        var memberDto = new PersonDto { Apartment = AnyApartment };

        // Assert
        memberDto.Apartment.ShouldBe(AnyApartment);
    }
    
    [Fact]
    public void GivenAnyStreet_WhenNewMemberDto_ThenStreetShouldBeSameAsGivenStreet()
    {
        // Act
        var memberDto = new PersonDto { Street = AnyStreet };

        // Assert
        memberDto.Street.ShouldBe(AnyStreet);
    }
    
    [Fact]
    public void GivenAnyCity_WhenNewMemberDto_ThenCityShouldBeSameAsGivenCity()
    {
        // Act
        var memberDto = new PersonDto { City = AnyCity };

        // Assert
        memberDto.City.ShouldBe(AnyCity);
    }
    
    [Fact]
    public void GivenAnyZipCode_WhenNewMemberDto_ThenZipCodeShouldBeSameAsGivenZipCode()
    {
        // Act
        var memberDto = new PersonDto { ZipCode = AnyZipCode };

        // Assert
        memberDto.ZipCode.ShouldBe(AnyZipCode);
    }
}