namespace Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public static bool operator ==(ValueObject? one, ValueObject? two)
    {
        return EqualOperator(one, two);
    }

    public static bool operator !=(ValueObject? one, ValueObject? two)
    {
        return NotEqualOperator(one, two);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    private static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        // ^ returns true if and only if exactly one of its operands is true.
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            return false;

        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        return ReferenceEquals(left, right) || left!.Equals(right);
    }

    private static bool NotEqualOperator(ValueObject? left, ValueObject? right)
    {
        return !(EqualOperator(left, right));
    }

}