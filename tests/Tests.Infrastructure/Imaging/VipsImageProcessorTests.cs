using Application.Interfaces.Imaging;
using Infrastructure.Imaging;
using Microsoft.AspNetCore.Http;
using NetVips;

namespace Tests.Infrastructure.Imaging;

public class VipsImageProcessorTests
{
    private static IFormFile MakeImageFormFile(int width, int height, string extension, string contentType)
    {
        using var image = Image.Black(width, height).NewFromImage([255, 0, 0]);
        var bytes = image.WriteToBuffer($".{extension}");
        var ms = new MemoryStream(bytes);
        ms.Position = 0;

        return new FormFile(ms, 0, ms.Length, "file", $"test.{extension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static Image Decode(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.Position = 0;
        stream.CopyTo(ms);
        return Image.NewFromBuffer(ms.ToArray(), string.Empty);
    }

    [Fact]
    public async Task GivenJpeg_WhenProcess_ThenReturnsThreeStreamsAndDimensions()
    {
        var sut = new VipsImageProcessor();
        var file = MakeImageFormFile(1000, 800, "jpg", "image/jpeg");

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
        result.DisplayContentType.ShouldBe("image/jpeg");
        result.DisplayFileExtension.ShouldBe("jpg");
        result.ThumbnailContentType.ShouldBe("image/jpeg");
        result.ThumbnailFileExtension.ShouldBe("jpg");
    }

    [Fact]
    public async Task GivenLargeImage_WhenProcess_ThenDisplayIsClampedTo2048LongSide()
    {
        var sut = new VipsImageProcessor();
        var file = MakeImageFormFile(4000, 2000, "jpg", "image/jpeg");

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        using var displayImage = Decode(result.DisplayStream);
        displayImage.Width.ShouldBe(2048);
        displayImage.Height.ShouldBe(1024);
    }

    [Fact]
    public async Task GivenSmallImage_WhenProcess_ThenDisplayIsNotUpscaled()
    {
        var sut = new VipsImageProcessor();
        var file = MakeImageFormFile(800, 600, "png", "image/png");

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        using var displayImage = Decode(result.DisplayStream);
        displayImage.Width.ShouldBe(800);
        displayImage.Height.ShouldBe(600);
    }

    [Fact]
    public async Task GivenAnyImage_WhenProcess_ThenThumbnailIsClampedTo400LongSide()
    {
        var sut = new VipsImageProcessor();
        var file = MakeImageFormFile(2000, 1000, "jpg", "image/jpeg");

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        using var thumbnailImage = Decode(result.ThumbnailStream);
        thumbnailImage.Width.ShouldBe(400);
        thumbnailImage.Height.ShouldBe(200);
    }

    [Fact]
    public async Task GivenAvif_WhenProcess_ThenReturnsJpegDisplayFallback()
    {
        var sut = new VipsImageProcessor();
        var file = MakeImageFormFile(1000, 800, "avif", "image/avif");

        using var result = await sut.ProcessImageAsync(file, CancellationToken.None);

        result.OriginalContentType.ShouldBe("image/avif");
        result.OriginalFileExtension.ShouldBe("avif");
        result.DisplayContentType.ShouldBe("image/jpeg");
        result.DisplayFileExtension.ShouldBe("jpg");
    }

    [Fact]
    public async Task GivenCorruptFile_WhenProcess_ThenThrowsInvalidImageException()
    {
        var sut = new VipsImageProcessor();
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
