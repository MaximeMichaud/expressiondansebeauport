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

        // Read EXIF orientation before decoding so we can apply it manually — re-encoding
        // to webp strips the EXIF tag, so pixels must be physically rotated here.
        SKEncodedOrigin origin;
        try
        {
            using var codec = SKCodec.Create(new MemoryStream(imageBytes));
            origin = codec?.EncodedOrigin ?? SKEncodedOrigin.TopLeft;
        }
        catch
        {
            origin = SKEncodedOrigin.TopLeft;
        }

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

        // Apply EXIF orientation if needed.
        if (origin != SKEncodedOrigin.TopLeft && origin != SKEncodedOrigin.Default)
        {
            var oriented = ApplyOrientation(bitmap, origin);
            bitmap.Dispose();
            bitmap = oriented;
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

    private static SKBitmap ApplyOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        var rotated = origin switch
        {
            SKEncodedOrigin.TopRight => Flip(bitmap, horizontal: true, vertical: false),
            SKEncodedOrigin.BottomRight => Rotate(bitmap, 180),
            SKEncodedOrigin.BottomLeft => Flip(bitmap, horizontal: false, vertical: true),
            SKEncodedOrigin.LeftTop => RotateAndFlip(bitmap, 90),
            SKEncodedOrigin.RightTop => Rotate(bitmap, 90),
            SKEncodedOrigin.RightBottom => RotateAndFlip(bitmap, 270),
            SKEncodedOrigin.LeftBottom => Rotate(bitmap, 270),
            _ => null
        };
        return rotated ?? bitmap;
    }

    private static SKBitmap Rotate(SKBitmap source, int degrees)
    {
        var radians = Math.PI * degrees / 180;
        var sin = (float)Math.Abs(Math.Sin(radians));
        var cos = (float)Math.Abs(Math.Cos(radians));
        var newWidth = (int)Math.Round(source.Width * cos + source.Height * sin);
        var newHeight = (int)Math.Round(source.Width * sin + source.Height * cos);
        var rotated = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(rotated);
        canvas.Translate(newWidth / 2f, newHeight / 2f);
        canvas.RotateDegrees(degrees);
        canvas.Translate(-source.Width / 2f, -source.Height / 2f);
        canvas.DrawBitmap(source, 0, 0);
        return rotated;
    }

    private static SKBitmap Flip(SKBitmap source, bool horizontal, bool vertical)
    {
        var flipped = new SKBitmap(source.Width, source.Height);
        using var canvas = new SKCanvas(flipped);
        canvas.Scale(horizontal ? -1 : 1, vertical ? -1 : 1, source.Width / 2f, source.Height / 2f);
        canvas.DrawBitmap(source, 0, 0);
        return flipped;
    }

    private static SKBitmap RotateAndFlip(SKBitmap source, int degrees)
    {
        using var rotated = Rotate(source, degrees);
        return Flip(rotated, horizontal: true, vertical: false);
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
