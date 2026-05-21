using System.Collections.Concurrent;
using Application.Interfaces.Imaging;
using NetVips;

namespace Infrastructure.Imaging;

public class VipsImageVariantGenerator : IImageVariantGenerator
{
    private static readonly IReadOnlyCollection<VipsImageFormat> Variants =
        [VipsImageFormats.Avif, VipsImageFormats.Webp];

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> VariantExtensions { get; } =
        Variants.Select(x => x.Extension).ToArray();

    public bool IsSupportedSourcePath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return VipsImageFormats.SupportedSourceExtensions.Contains(extension)
            && !IsGeneratedVariantPath(filePath);
    }

    public string GetVariantPath(string sourcePath, string variantExtension)
    {
        var extension = NormalizeVariantExtension(variantExtension);

        if (!VariantExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported image variant extension.", nameof(variantExtension));
        }

        return sourcePath + extension;
    }

    public bool HasCurrentVariant(string sourcePath, string variantExtension)
    {
        if (!IsSupportedSourcePath(sourcePath) || !File.Exists(sourcePath))
        {
            return false;
        }

        var variantPath = GetVariantPath(sourcePath, variantExtension);
        return IsCurrent(variantPath, File.GetLastWriteTimeUtc(sourcePath));
    }

    public async Task EnsureVariantsAsync(string sourcePath, CancellationToken ct)
    {
        if (!IsSupportedSourcePath(sourcePath) || !File.Exists(sourcePath))
        {
            return;
        }

        var gate = _locks.GetOrAdd(sourcePath, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            EnsureVariantsCore(sourcePath, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<bool> TryEnsureVariantsAsync(string sourcePath, CancellationToken ct)
    {
        if (!IsSupportedSourcePath(sourcePath) || !File.Exists(sourcePath))
        {
            return false;
        }

        var gate = _locks.GetOrAdd(sourcePath, _ => new SemaphoreSlim(1, 1));
        if (!await gate.WaitAsync(0, ct))
        {
            return false;
        }

        try
        {
            EnsureVariantsCore(sourcePath, ct);
            return true;
        }
        finally
        {
            gate.Release();
        }
    }

    private void EnsureVariantsCore(string sourcePath, CancellationToken ct)
    {
        var sourceExtension = Path.GetExtension(sourcePath).ToLowerInvariant();
        var sourceLastWriteUtc = File.GetLastWriteTimeUtc(sourcePath);
        var missingVariants = Variants
            .Where(variant => variant.Extension != sourceExtension)
            .Where(variant => !IsCurrent(GetVariantPath(sourcePath, variant.Extension), sourceLastWriteUtc))
            .ToArray();

        if (missingVariants.Length == 0)
        {
            return;
        }

        using var image = VipsImageFormats.LoadFromFile(sourcePath);
        using var webImage = VipsImageFormats.PrepareForWeb(image);

        foreach (var variant in missingVariants)
        {
            ct.ThrowIfCancellationRequested();
            WriteVariant(webImage, sourcePath, sourceLastWriteUtc, variant);
        }
    }

    private static bool IsCurrent(string variantPath, DateTime sourceLastWriteUtc)
    {
        return File.Exists(variantPath)
            && new FileInfo(variantPath).Length > 0
            && File.GetLastWriteTimeUtc(variantPath) >= sourceLastWriteUtc;
    }

    private static bool IsGeneratedVariantPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension is not ".webp" and not ".avif")
        {
            return false;
        }

        var sourcePath = filePath[..^extension.Length];
        var sourceExtension = Path.GetExtension(sourcePath).ToLowerInvariant();
        return VipsImageFormats.SupportedSourceExtensions.Contains(sourceExtension)
            && File.Exists(sourcePath);
    }

    private void WriteVariant(Image image, string sourcePath, DateTime sourceLastWriteUtc, VipsImageFormat variant)
    {
        var variantPath = GetVariantPath(sourcePath, variant.Extension);
        var tempPath = $"{variantPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var bytes = image.WriteToBuffer(variant.BufferSuffix);
            File.WriteAllBytes(tempPath, bytes);
            File.SetLastWriteTimeUtc(tempPath, sourceLastWriteUtc);
            File.Move(tempPath, variantPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string NormalizeVariantExtension(string variantExtension)
    {
        return variantExtension.StartsWith('.')
            ? variantExtension.ToLowerInvariant()
            : $".{variantExtension.ToLowerInvariant()}";
    }
}
