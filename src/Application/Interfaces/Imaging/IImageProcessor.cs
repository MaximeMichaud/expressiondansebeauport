using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Imaging;

public interface IImageProcessor
{
    /// <summary>
    /// Decodes the input image and produces the untouched original, a display image and a thumbnail image.
    /// Throws InvalidImageException if the file is not a decodable image.
    /// </summary>
    Task<ProcessedImage> ProcessImageAsync(IFormFile file, CancellationToken ct);
}
