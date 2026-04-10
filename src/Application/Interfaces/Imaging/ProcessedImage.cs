namespace Application.Interfaces.Imaging;

public sealed record ProcessedImage(
    Stream OriginalStream,
    string OriginalContentType,
    string OriginalFileExtension,
    Stream DisplayStream,
    Stream ThumbnailStream,
    int Width,
    int Height) : IDisposable
{
    public void Dispose()
    {
        OriginalStream.Dispose();
        DisplayStream.Dispose();
        ThumbnailStream.Dispose();
    }
}
