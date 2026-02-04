using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects;

public class ValueObjectTests
{
    private const string ANY_PHONE_NUMBER = "514-640-8920";

    private readonly ValueObject? _valueObject = new PhoneNumber(string.Empty);

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNull_WhenEqual_ThenReturnTrue()
    {
        // Act
        var areEqual = null == null as ValueObject;

        // Assert
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void GivenLeftValueObjectIsNullAndRightIsNotNull_WhenEqual_ThenReturnFalse()
    {
        // Act
        var areEqual = null == _valueObject;

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void GivenRightValueObjectIsNullAndLeftIsNotNull_WhenEqual_ThenReturnFalse()
    {
        // Act
        var areEqual = _valueObject == null;

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNotNullAndPropertiesMatch_WhenEqual_ThenReturnTrue()
    {
        // Act
        var areEqual = _valueObject == new PhoneNumber(string.Empty);

        // Assert
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNotNullAndPropertiesDontMatch_WhenEqual_ThenReturnFalse()
    {
        // Act
        var areEqual = _valueObject == new PhoneNumber(ANY_PHONE_NUMBER);

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNull_WhenNotEqual_ThenReturnFalse()
    {
        // Act
        var areEqual = null != null as ValueObject;

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void GivenLeftValueObjectIsNullAndRightIsNotNull_WhenNotEqual_ThenReturnTrue()
    {
        // Act
        var areEqual = null != _valueObject;

        // Assert
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void GivenRightValueObjectIsNullAndLeftIsNotNull_WhenNotEqual_ThenReturnTrue()
    {
        // Act
        var areEqual = _valueObject != null;

        // Assert
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNotNullAndPropertiesMatch_WhenNotEqual_ThenReturnFalse()
    {
        // Act
        var areEqual = _valueObject != new PhoneNumber(string.Empty);

        // Assert
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void GivenLeftAndRightValueObjectsAreNotNullAndPropertiesDontMatch_WhenNotEqual_ThenReturnTrue()
    {
        // Act
        var areEqual = _valueObject != new PhoneNumber(ANY_PHONE_NUMBER);

        // Assert
        areEqual.ShouldBeTrue();
    }
}