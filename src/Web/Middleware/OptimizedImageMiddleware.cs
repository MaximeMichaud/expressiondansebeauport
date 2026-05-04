using Application.Interfaces.Imaging;
using Microsoft.Net.Http.Headers;

namespace Web.Middleware;

public class OptimizedImageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _rootPath;
    private readonly IImageVariantGenerator _imageVariantGenerator;
    private readonly ILogger<OptimizedImageMiddleware> _logger;

    public OptimizedImageMiddleware(
        RequestDelegate next,
        string rootPath,
        IImageVariantGenerator imageVariantGenerator,
        ILogger<OptimizedImageMiddleware> logger)
    {
        _next = next;
        _rootPath = Path.GetFullPath(rootPath);
        _imageVariantGenerator = imageVariantGenerator;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var sourcePath = GetPhysicalPath(context.Request.Path);
        if (sourcePath == null || !_imageVariantGenerator.IsSupportedSourcePath(sourcePath) || !File.Exists(sourcePath))
        {
            await _next(context);
            return;
        }

        context.Response.OnStarting(() =>
        {
            AddVaryAccept(context.Response.Headers);
            return Task.CompletedTask;
        });

        var preferredExtension = ResolvePreferredVariant(context, sourcePath);
        if (preferredExtension == null)
        {
            await _next(context);
            return;
        }

        var variantPath = _imageVariantGenerator.GetVariantPath(sourcePath, preferredExtension);
        if (_imageVariantGenerator.HasCurrentVariant(sourcePath, preferredExtension))
        {
            var relativeVariantPath = Path.GetRelativePath(_rootPath, variantPath)
                .Replace(Path.DirectorySeparatorChar, '/');
            context.Request.Path = "/" + relativeVariantPath;
        }
        else
        {
            QueueVariantGeneration(sourcePath);
        }

        await _next(context);
    }

    private void QueueVariantGeneration(string sourcePath)
    {
        _ = Task.Run(() => TryGenerateVariantsAsync(sourcePath), CancellationToken.None);
    }

    private async Task TryGenerateVariantsAsync(string sourcePath)
    {
        try
        {
            await _imageVariantGenerator.TryEnsureVariantsAsync(sourcePath, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to generate optimized image variants for {ImagePath}", sourcePath);
        }
    }

    private string? GetPhysicalPath(PathString requestPath)
    {
        var path = requestPath.Value;
        if (string.IsNullOrWhiteSpace(path) || path.Contains('\0'))
        {
            return null;
        }

        var relativePath = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        return IsInsideRoot(physicalPath) ? physicalPath : null;
    }

    private bool IsInsideRoot(string path)
    {
        return path.Equals(_rootPath, StringComparison.Ordinal)
            || path.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    private static string? ResolvePreferredVariant(HttpContext context, string sourcePath)
    {
        var sourceExtension = Path.GetExtension(sourcePath).ToLowerInvariant();
        var avifQuality = GetAcceptedQuality(context, "image/avif");
        var webpQuality = GetAcceptedQuality(context, "image/webp");

        if (avifQuality > 0 && avifQuality >= webpQuality && sourceExtension != ".avif")
        {
            return ".avif";
        }

        if (webpQuality > 0 && sourceExtension != ".webp")
        {
            return ".webp";
        }

        return null;
    }

    private static double GetAcceptedQuality(HttpContext context, string contentType)
    {
        var accept = context.Request.GetTypedHeaders().Accept;
        if (accept == null)
        {
            return 0;
        }

        return accept
            .Where(x => x.MediaType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Quality ?? 1)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static void AddVaryAccept(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderNames.Vary, out var current) || string.IsNullOrWhiteSpace(current))
        {
            headers[HeaderNames.Vary] = HeaderNames.Accept;
            return;
        }

        var values = current.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (values.Any(x => x.Equals(HeaderNames.Accept, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        headers[HeaderNames.Vary] = current + ", " + HeaderNames.Accept;
    }
}
