using System.Diagnostics;
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
            generator.Setup(x => x.HasCurrentVariant(sourcePath, ".avif")).Returns(true);

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
            generator.Verify(x => x.TryEnsureVariantsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
            generator.Verify(x => x.TryEnsureVariantsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenVariantMissing_WhenImageRequested_ThenKeepsOriginalPathAndDoesNotWaitForGeneration()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-optimized-middleware-");
        var generationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var generationCanFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            var avifPath = sourcePath + ".avif";
            await File.WriteAllBytesAsync(sourcePath, [1]);

            PathString? pathAtNext = null;
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.WebRootPath).Returns(tempDir.FullName);
            environment.SetupGet(x => x.ContentRootPath).Returns(tempDir.FullName);

            var generator = new Mock<IImageVariantGenerator>();
            generator.Setup(x => x.IsSupportedSourcePath(sourcePath)).Returns(true);
            generator.Setup(x => x.GetVariantPath(sourcePath, ".avif")).Returns(avifPath);
            generator.Setup(x => x.HasCurrentVariant(sourcePath, ".avif")).Returns(false);
            generator.Setup(x => x.TryEnsureVariantsAsync(sourcePath, It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    generationStarted.TrySetResult();
                    await generationCanFinish.Task;
                    return true;
                });

            var middleware = new OptimizedImageMiddleware(
                _ =>
                {
                    pathAtNext = _.Request.Path;
                    return Task.CompletedTask;
                },
                environment.Object,
                generator.Object,
                NullLogger<OptimizedImageMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/photo.jpg";
            context.Request.Headers.Accept = "image/avif,image/webp";

            var elapsed = Stopwatch.StartNew();
            await middleware.InvokeAsync(context);
            elapsed.Stop();

            pathAtNext?.Value.ShouldBe("/photo.jpg");
            elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
            (await Task.WhenAny(generationStarted.Task, Task.Delay(TimeSpan.FromSeconds(1)))).ShouldBe(generationStarted.Task);
        }
        finally
        {
            generationCanFinish.TrySetResult(true);
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenStaleVariant_WhenImageRequested_ThenKeepsOriginalPath()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-optimized-middleware-");
        try
        {
            var sourcePath = Path.Combine(tempDir.FullName, "photo.jpg");
            var avifPath = sourcePath + ".avif";
            await File.WriteAllBytesAsync(sourcePath, [1]);
            await File.WriteAllBytesAsync(avifPath, [1]);

            PathString? pathAtNext = null;
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.WebRootPath).Returns(tempDir.FullName);
            environment.SetupGet(x => x.ContentRootPath).Returns(tempDir.FullName);

            var generator = new Mock<IImageVariantGenerator>();
            generator.Setup(x => x.IsSupportedSourcePath(sourcePath)).Returns(true);
            generator.Setup(x => x.GetVariantPath(sourcePath, ".avif")).Returns(avifPath);
            generator.Setup(x => x.HasCurrentVariant(sourcePath, ".avif")).Returns(false);
            generator.Setup(x => x.TryEnsureVariantsAsync(sourcePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var middleware = new OptimizedImageMiddleware(
                _ =>
                {
                    pathAtNext = _.Request.Path;
                    return Task.CompletedTask;
                },
                environment.Object,
                generator.Object,
                NullLogger<OptimizedImageMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/photo.jpg";
            context.Request.Headers.Accept = "image/avif,image/webp";

            await middleware.InvokeAsync(context);

            pathAtNext?.Value.ShouldBe("/photo.jpg");
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
