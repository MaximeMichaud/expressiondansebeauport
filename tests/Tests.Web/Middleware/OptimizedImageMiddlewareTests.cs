using Application.Interfaces.Imaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Web.Middleware;

namespace Tests.Web.Middleware;

public class OptimizedImageMiddlewareTests
{
    [Fact]
    public async Task GivenBrowserAcceptsAvif_WhenImageRequested_ThenRewritesToAvifVariant()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-optimized-middleware-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            var avifPath = sourcePath + ".avif";
            await File.WriteAllBytesAsync(sourcePath, [1]);
            await File.WriteAllBytesAsync(avifPath, [1]);

            PathString? pathAtNext = null;

            var generator = new Mock<IImageVariantGenerator>();
            generator.Setup(x => x.IsSupportedSourcePath(sourcePath)).Returns(true);
            generator.Setup(x => x.GetVariantPath(sourcePath, ".avif")).Returns(avifPath);
            generator.Setup(x => x.EnsureVariantsAsync(sourcePath, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var middleware = new OptimizedImageMiddleware(
                _ =>
                {
                    pathAtNext = _.Request.Path;
                    return Task.CompletedTask;
                },
                tempDir.FullName,
                generator.Object,
                NullLogger<OptimizedImageMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/photo.jpg";
            context.Request.Headers.Accept = "image/avif,image/webp";

            await middleware.InvokeAsync(context);

            pathAtNext?.Value.ShouldBe("/photo.jpg.avif");
            generator.Verify(x => x.EnsureVariantsAsync(sourcePath, It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenBrowserDoesNotAcceptModernFormats_WhenImageRequested_ThenKeepsOriginalPath()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-optimized-middleware-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            await File.WriteAllBytesAsync(sourcePath, [1]);

            PathString? pathAtNext = null;

            var generator = new Mock<IImageVariantGenerator>();
            generator.Setup(x => x.IsSupportedSourcePath(sourcePath)).Returns(true);

            var middleware = new OptimizedImageMiddleware(
                _ =>
                {
                    pathAtNext = _.Request.Path;
                    return Task.CompletedTask;
                },
                tempDir.FullName,
                generator.Object,
                NullLogger<OptimizedImageMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/photo.jpg";
            context.Request.Headers.Accept = "image/png";

            await middleware.InvokeAsync(context);

            pathAtNext?.Value.ShouldBe("/photo.jpg");
            generator.Verify(x => x.EnsureVariantsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
