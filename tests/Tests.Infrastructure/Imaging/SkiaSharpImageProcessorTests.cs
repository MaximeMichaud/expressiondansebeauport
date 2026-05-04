using Application.Interfaces.Imaging;
using Infrastructure.Imaging;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace Tests.Infrastructure.Imaging;

public class SkiaSharpImageProcessorTests
{
    private static IFormFile MakeImageFormFile(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Red);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);

        var ms = new MemoryStream(data.ToArray());
        ms.Position = 0;

        var contentType = format switch
        {
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => "application/octet-stream"
        };
        var ext = format switch
        {
            SKEncodedImageFormat.Jpeg => "jpg",
            SKEncodedImageFormat.Png => "png",
            SKEncodedImageFormat.Webp => "webp",
            _ => "bin"
        };

        return new FormFile(ms, 0, ms.Length, "file", $"test.{ext}")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task GivenJpeg_WhenProcess_ThenReturnsThreeStreamsAndDimensions()
    {
        var sut = new SkiaSharpImageProcessor();
        var file = MakeImageFormFile(1000, 800, SKEncodedImageFormat.Jpeg);

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        result.OriginalStream.ShouldNotBeNull();
        result.OriginalStream.Length.ShouldBeGreaterThan(0);
        result.DisplayStream.ShouldNotBeNull();
        result.DisplayStream.Length.ShouldBeGreaterThan(0);
        result.ThumbnailStream.ShouldNotBeNull();
        result.ThumbnailStream.Length.ShouldBeGreaterThan(0);
        result.Width.ShouldBe(1000);
        result.Height.ShouldBe(800);
        result.OriginalContentType.ShouldBe("image/jpeg");
        result.OriginalFileExtension.ShouldBe("jpg");
    }

    [Fact]
    public async Task GivenLargeImage_WhenProcess_ThenDisplayIsClampedTo2048LongSide()
    {
        var sut = new SkiaSharpImageProcessor();
        var file = MakeImageFormFile(4000, 2000, SKEncodedImageFormat.Jpeg);

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        result.DisplayStream.Position = 0;
        using var displayBitmap = SKBitmap.Decode(result.DisplayStream);
        displayBitmap.Width.ShouldBe(2048);
        displayBitmap.Height.ShouldBe(1024);
    }

    [Fact]
    public async Task GivenSmallImage_WhenProcess_ThenDisplayIsNotUpscaled()
    {
        var sut = new SkiaSharpImageProcessor();
        var file = MakeImageFormFile(800, 600, SKEncodedImageFormat.Png);

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        result.DisplayStream.Position = 0;
        using var displayBitmap = SKBitmap.Decode(result.DisplayStream);
        displayBitmap.Width.ShouldBe(800);
        displayBitmap.Height.ShouldBe(600);
    }

    [Fact]
    public async Task GivenAnyImage_WhenProcess_ThenThumbnailIsClampedTo400LongSide()
    {
        var sut = new SkiaSharpImageProcessor();
        var file = MakeImageFormFile(2000, 1000, SKEncodedImageFormat.Jpeg);

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        result.ThumbnailStream.Position = 0;
        using var thumbBitmap = SKBitmap.Decode(result.ThumbnailStream);
        thumbBitmap.Width.ShouldBe(400);
        thumbBitmap.Height.ShouldBe(200);
    }

    [Fact]
    public async Task GivenCorruptFile_WhenProcess_ThenThrowsInvalidImageException()
    {
        var sut = new SkiaSharpImageProcessor();
        var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
        var ms = new MemoryStream(bytes);
        var file = new FormFile(ms, 0, ms.Length, "file", "broken.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        await Should.ThrowAsync<InvalidImageException>(
            () => sut.ProcessImageAsync(file, CancellationToken.None));
    }
}
