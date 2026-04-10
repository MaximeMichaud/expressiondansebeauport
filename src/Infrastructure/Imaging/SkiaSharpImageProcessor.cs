using Application.Interfaces.Imaging;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace Infrastructure.Imaging;

public class SkiaSharpImageProcessor : IImageProcessor
{
    private const int DisplayMaxLongSide = 2048;
    private const int DisplayQuality = 80;
    private const int ThumbnailMaxLongSide = 400;
    private const int ThumbnailQuality = 70;

    public async Task<ProcessedImage> ProcessImageAsync(IFormFile file, CancellationToken ct)
    {
        var originalBuffer = new MemoryStream();
        await using (var input = file.OpenReadStream())
        {
            await input.CopyToAsync(originalBuffer, ct);
        }
        originalBuffer.Position = 0;

        // Decode from byte array — SKBitmap.Decode(Stream) disposes the stream internally.
        var imageBytes = originalBuffer.ToArray();

        SKBitmap? bitmap;
        try
        {
            bitmap = SKBitmap.Decode(imageBytes);
        }
        catch (Exception ex)
        {
            originalBuffer.Dispose();
            throw new InvalidImageException("Image could not be decoded.", ex);
        }

        if (bitmap == null)
        {
            originalBuffer.Dispose();
            throw new InvalidImageException("Image could not be decoded.");
        }

        MemoryStream? displayStream = null;
        MemoryStream? thumbnailStream = null;
        try
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            displayStream = EncodeResized(bitmap, DisplayMaxLongSide, DisplayQuality);
            thumbnailStream = EncodeResized(bitmap, ThumbnailMaxLongSide, ThumbnailQuality);

            originalBuffer.Position = 0;

            var (contentType, ext) = NormalizeContentType(file.ContentType, file.FileName);

            return new ProcessedImage(
                OriginalStream: originalBuffer,
                OriginalContentType: contentType,
                OriginalFileExtension: ext,
                DisplayStream: displayStream,
                ThumbnailStream: thumbnailStream,
                Width: width,
                Height: height);
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
            bitmap.Dispose();
        }
    }

    private static MemoryStream EncodeResized(SKBitmap source, int maxLongSide, int quality)
    {
        var width = source.Width;
        var height = source.Height;
        var longSide = Math.Max(width, height);

        SKBitmap target;
        bool ownsTarget;
        if (longSide <= maxLongSide)
        {
            target = source;
            ownsTarget = false;
        }
        else
        {
            var scale = (float)maxLongSide / longSide;
            var newWidth = (int)Math.Round(width * scale);
            var newHeight = (int)Math.Round(height * scale);
            target = source.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (target == null)
            {
                throw new InvalidImageException("Image resize failed.");
            }
            ownsTarget = true;
        }

        try
        {
            using var image = SKImage.FromBitmap(target);
            using var data = image.Encode(SKEncodedImageFormat.Webp, quality);
            var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;
            return ms;
        }
        finally
        {
            if (ownsTarget) target.Dispose();
        }
    }

    private static (string ContentType, string Extension) NormalizeContentType(string browserContentType, string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return browserContentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ("image/jpeg", "jpg"),
            "image/png" => ("image/png", "png"),
            "image/webp" => ("image/webp", "webp"),
            "image/gif" => ("image/gif", "gif"),
            _ => string.IsNullOrEmpty(ext)
                ? ("application/octet-stream", "bin")
                : ($"image/{ext}", ext)
        };
    }
}
