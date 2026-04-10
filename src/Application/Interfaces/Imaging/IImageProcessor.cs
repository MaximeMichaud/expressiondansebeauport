using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Imaging;

public interface IImageProcessor
{
    /// <summary>
    /// Decodes the input image, produces 3 streams: untouched original, display webp (max 2048px long side, q80),
    /// thumbnail webp (max 400px long side, q70). Throws InvalidImageException if the file is not a decodable image.
    /// </summary>
    Task<ProcessedImage> ProcessImageAsync(IFormFile file, CancellationToken ct);
}
