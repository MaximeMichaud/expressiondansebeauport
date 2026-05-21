using Infrastructure.Imaging;
using NetVips;

namespace Tests.Infrastructure.Imaging;

public class VipsImageVariantGeneratorTests
{
    [Fact]
    public async Task GivenSupportedImage_WhenEnsureVariants_ThenCreatesWebpAndAvifFiles()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-image-variants-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            using (var image = Image.Black(32, 24).NewFromImage([255, 0, 0]))
            {
                image.WriteToFile(sourcePath);
            }

            var sut = new VipsImageVariantGenerator();

            await sut.EnsureVariantsAsync(sourcePath, CancellationToken.None);

            var webpPath = sourcePath + ".webp";
            var avifPath = sourcePath + ".avif";

            File.Exists(webpPath).ShouldBeTrue();
            File.Exists(avifPath).ShouldBeTrue();
            new FileInfo(webpPath).Length.ShouldBeGreaterThan(0);
            new FileInfo(avifPath).Length.ShouldBeGreaterThan(0);

            using var webp = Image.NewFromFile(webpPath);
            using var avif = Image.NewFromFile(avifPath);
            webp.Width.ShouldBe(32);
            webp.Height.ShouldBe(24);
            avif.Width.ShouldBe(32);
            avif.Height.ShouldBe(24);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenExistingCurrentVariants_WhenEnsureVariants_ThenKeepsFiles()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-image-variants-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            using (var image = Image.Black(32, 24).NewFromImage([255, 0, 0]))
            {
                image.WriteToFile(sourcePath);
            }

            var sut = new VipsImageVariantGenerator();
            await sut.EnsureVariantsAsync(sourcePath, CancellationToken.None);
            var webpPath = sourcePath + ".webp";
            var firstWrite = File.GetLastWriteTimeUtc(webpPath);

            await sut.EnsureVariantsAsync(sourcePath, CancellationToken.None);

            File.GetLastWriteTimeUtc(webpPath).ShouldBe(firstWrite);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenCurrentVariant_WhenHasCurrentVariant_ThenReturnsTrue()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-image-variants-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            using (var image = Image.Black(32, 24).NewFromImage([255, 0, 0]))
            {
                image.WriteToFile(sourcePath);
            }

            var sut = new VipsImageVariantGenerator();
            await sut.EnsureVariantsAsync(sourcePath, CancellationToken.None);

            sut.HasCurrentVariant(sourcePath, ".webp").ShouldBeTrue();
            sut.HasCurrentVariant(sourcePath, ".avif").ShouldBeTrue();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void GivenMissingVariant_WhenHasCurrentVariant_ThenReturnsFalse()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-image-variants-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            File.WriteAllBytes(sourcePath, [1]);

            var sut = new VipsImageVariantGenerator();

            sut.HasCurrentVariant(sourcePath, ".webp").ShouldBeFalse();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void GivenGeneratedVariantPath_WhenSourceExists_ThenPathIsNotSupportedAsSource()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-image-variants-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            var variantPath = sourcePath + ".webp";
            File.WriteAllBytes(sourcePath, [1]);
            File.WriteAllBytes(variantPath, [1]);

            var sut = new VipsImageVariantGenerator();

            sut.IsSupportedSourcePath(variantPath).ShouldBeFalse();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
