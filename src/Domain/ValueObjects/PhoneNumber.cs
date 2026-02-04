using Domain.Entities;
using Domain.Helpers;

namespace Domain.ValueObjects;

public class PhoneNumber : ValueObject, ISanitizable
{
    public const string EXTENSION_SEPARATOR = "poste";

    public string Number { get; private set; }
    public int? Extension { get; private set; }

    public PhoneNumber(string? numberWithExtension)
    {
        Number = PhoneNumberHelper.RemoveExtensionFromPhoneNumber(numberWithExtension) ?? null!;
        Extension = PhoneNumberHelper.FindExtensionInPhoneNumber(numberWithExtension);
    }

    public PhoneNumber(string? number, int? extension)
    {
        Number = number ?? null!;
        Extension = extension;
    }

    public void Update(string? number, int? extension)
    {
        Number = number ?? Number;
        Extension = extension;
    }

    public override string? ToString()
    {
        return Extension.HasValue ? $"{Number} {EXTENSION_SEPARATOR} {Extension}" : Number;
    }

    public string? ToShortString()
    {
        return Extension.HasValue ? $"{Number}, {Extension}" : Number;
    }

    public void SanitizeForSaving()
    {
        Number = Number?.Trim() ?? string.Empty;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Number;
        yield return Extension;
    }
}