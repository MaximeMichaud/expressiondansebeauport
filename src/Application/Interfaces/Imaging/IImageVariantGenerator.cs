namespace Application.Interfaces.Imaging;

public interface IImageVariantGenerator
{
    IReadOnlyCollection<string> VariantExtensions { get; }

    bool IsSupportedSourcePath(string filePath);

    string GetVariantPath(string sourcePath, string variantExtension);

    Task EnsureVariantsAsync(string sourcePath, CancellationToken ct);
}
