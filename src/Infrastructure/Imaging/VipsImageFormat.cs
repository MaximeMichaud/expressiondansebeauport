using NetVips;

namespace Infrastructure.Imaging;

internal sealed record VipsImageFormat(string Extension, string ContentType, int Quality)
{
    public string BufferSuffix => Extension switch
    {
        ".jpg" => $".jpg[Q={Quality}]",
        ".jpeg" => $".jpg[Q={Quality}]",
        ".png" => ".png[compression=9]",
        ".webp" => $".webp[Q={Quality}]",
        ".avif" => $".avif[Q={Quality}]",
        _ => Extension
    };
}

internal static class VipsImageFormats
{
    public static readonly VipsImageFormat Jpeg = new(".jpg", "image/jpeg", 82);
    public static readonly VipsImageFormat Png = new(".png", "image/png", 0);
    public static readonly VipsImageFormat Webp = new(".webp", "image/webp", 80);
    public static readonly VipsImageFormat Avif = new(".avif", "image/avif", 60);

    public static readonly IReadOnlyCollection<string> SupportedSourceExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif"];

    public static Image LoadFromBuffer(byte[] buffer)
    {
        using var loaded = Image.NewFromBuffer(buffer, string.Empty, access: Enums.Access.Random);
        return loaded.Autorot();
    }

    public static Image LoadFromFile(string filePath)
    {
        using var loaded = Image.NewFromFile(filePath, access: Enums.Access.Random);
        return loaded.Autorot();
    }

    public static Image PrepareForWeb(Image image)
    {
        try
        {
            return image.Colourspace(Enums.Interpretation.Srgb);
        }
        catch
        {
            return image.Copy();
        }
    }
}
