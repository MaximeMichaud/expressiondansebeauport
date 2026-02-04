// ReSharper disable LocalizableElement
namespace Domain.Common;

public class TranslatableString
{
    public string Fr { get; set; } = null!;
    public string En { get; set; } = null!;

    public TranslatableString() { }

    public TranslatableString(string fr, string en)
    {
        Fr = fr;
        En = en;
    }

    public string Transl8(string? language = null)
    {
        var lookupLanguage = GetLookupLanguage(language);

        switch (lookupLanguage)
        {
            case "fr":
                return Fr;
            case "en":
                return En;
            default:
                if (string.IsNullOrWhiteSpace(language))
                {
                    throw new Exception($"Could not get translation. Language \"{lookupLanguage}\" " +
                                        "(current UI culture) is not managed.");
                }

                throw new ArgumentException(
                    $"Could not get translation. Language \"{lookupLanguage}\" is not managed.",
                    nameof(language));
        }
    }

    public bool IsEmpty(bool partially = false)
    {
        return partially
            ? string.IsNullOrWhiteSpace(Fr) || string.IsNullOrWhiteSpace(En)
            : string.IsNullOrWhiteSpace(Fr) && string.IsNullOrWhiteSpace(En);
    }

    public override string ToString() => Transl8();

    public bool Equals(TranslatableString other)
    {
        return Fr == other.Fr && En == other.En;
    }

    private string GetLookupLanguage(string? language)
    {
        var lookup = string.IsNullOrWhiteSpace(language)
            ? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
            : language.ToLowerInvariant();

        // `iv` is a special case where a build machine does not have a default culture set
        return lookup == "iv" ? "en" : lookup;
    }
}