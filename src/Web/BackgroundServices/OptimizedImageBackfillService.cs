using Application.Interfaces.Imaging;

namespace Web.BackgroundServices;

public class OptimizedImageBackfillService : BackgroundService
{
    private readonly string _scanRootPath;
    private readonly IImageVariantGenerator _imageVariantGenerator;
    private readonly ILogger<OptimizedImageBackfillService> _logger;

    public OptimizedImageBackfillService(
        string scanRootPath,
        IImageVariantGenerator imageVariantGenerator,
        ILogger<OptimizedImageBackfillService> logger)
    {
        _scanRootPath = scanRootPath;
        _imageVariantGenerator = imageVariantGenerator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(_scanRootPath))
        {
            return;
        }

        await Task.Yield();

        foreach (var filePath in Directory.EnumerateFiles(_scanRootPath, "*", SearchOption.AllDirectories))
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            if (!_imageVariantGenerator.IsSupportedSourcePath(filePath))
            {
                continue;
            }

            try
            {
                await _imageVariantGenerator.EnsureVariantsAsync(filePath, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to backfill optimized image variants for {ImagePath}", filePath);
            }
        }
    }
}
