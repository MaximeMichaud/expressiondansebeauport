using Application.Interfaces.Imaging;
using Microsoft.AspNetCore.Http;
using NetVips;

namespace Infrastructure.Imaging;

public class VipsImageProcessor : IImageProcessor
{
    private const int DisplayMaxLongSide = 2048;
    private const int ThumbnailMaxLongSide = 400;

    public async Task<ProcessedImage> ProcessImageAsync(IFormFile file, CancellationToken ct)
    {
        var originalBuffer = new MemoryStream();
        await using (var input = file.OpenReadStream())
        {
            await input.CopyToAsync(originalBuffer, ct);
        }

        var imageBytes = originalBuffer.ToArray();
        Image image;
        try
        {
            image = VipsImageFormats.LoadFromBuffer(imageBytes);
        }
        catch (Exception ex) when (ex is VipsException or TypeInitializationException)
        {
            originalBuffer.Dispose();
            throw new InvalidImageException("Image could not be decoded.", ex);
        }

        MemoryStream? displayStream = null;
        MemoryStream? thumbnailStream = null;
        try
        {
            var width = image.Width;
            var height = image.Height;
            var fallbackFormat = image.HasAlpha()
                ? VipsImageFormats.Png
                : VipsImageFormats.Jpeg;

            displayStream = EncodeResized(image, DisplayMaxLongSide, fallbackFormat);
            thumbnailStream = EncodeResized(image, ThumbnailMaxLongSide, fallbackFormat);

            originalBuffer.Position = 0;
            var (contentType, ext) = NormalizeContentType(file.ContentType, file.FileName);

            return new ProcessedImage(
                OriginalStream: originalBuffer,
                OriginalContentType: contentType,
                OriginalFileExtension: ext,
                DisplayStream: displayStream,
                DisplayContentType: fallbackFormat.ContentType,
                DisplayFileExtension: fallbackFormat.Extension.TrimStart('.'),
                ThumbnailStream: thumbnailStream,
                ThumbnailContentType: fallbackFormat.ContentType,
                ThumbnailFileExtension: fallbackFormat.Extension.TrimStart('.'),
                Width: width,
                Height: height);
        }
        catch (VipsException ex)
        {
            originalBuffer.Dispose();
            displayStream?.Dispose();
            thumbnailStream?.Dispose();
            throw new InvalidImageException("Image could not be processed.", ex);
        }
        catch
        {
            originalBuffer.Dispose();
            displayStream?.Dispose();
            thumbnailStream?.Dispose();
            throw;
        }
        finally
        {
            image.Dispose();
        }
    }

    private static MemoryStream EncodeResized(Image source, int maxLongSide, VipsImageFormat format)
    {
        using var resized = ResizeToLongSide(source, maxLongSide);
        using var webImage = VipsImageFormats.PrepareForWeb(resized);
        var bytes = webImage.WriteToBuffer(format.BufferSuffix);
        return new MemoryStream(bytes, writable: false);
    }

    private static Image ResizeToLongSide(Image source, int maxLongSide)
    {
        var longSide = Math.Max(source.Width, source.Height);
        if (longSide <= maxLongSide)
        {
            return source.Copy();
        }

        var scale = (double)maxLongSide / longSide;
        return source.Resize(scale, kernel: Enums.Kernel.Lanczos3);
    }

    private static (string ContentType, string Extension) NormalizeContentType(string browserContentType, string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return browserContentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ("image/jpeg", "jpg"),
            "image/png" => ("image/png", "png"),
            "image/webp" => ("image/webp", "webp"),
            "image/avif" => ("image/avif", "avif"),
            "image/gif" => ("image/gif", "gif"),
            _ => string.IsNullOrEmpty(ext)
                ? ("application/octet-stream", "bin")
                : ($"image/{ext}", ext)
        };
    }
}
