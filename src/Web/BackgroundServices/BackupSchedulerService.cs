using Application.Interfaces.Services;
using Domain.Entities;

namespace Web.BackgroundServices;

public class BackupSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupSchedulerService> _logger;
    private readonly IConfiguration _configuration;

    public BackupSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<BackupSchedulerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!bool.TryParse(_configuration["Backup:ScheduleEnabled"], out var enabled) || !enabled)
        {
            _logger.LogInformation("Backup automatique désactivé");
            return;
        }

        var intervalHours = int.TryParse(_configuration["Backup:ScheduleIntervalHours"], out var h) ? h : 24;

        // Attendre que l'app soit prête
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        _logger.LogInformation("Backup automatique activé - intervalle : {Hours}h", intervalHours);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));

        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                await backupService.CreateBackupAsync(BackupType.Scheduled, stoppingToken);
                _logger.LogInformation("Backup automatique terminé");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erreur lors du backup automatique");
            }
        }
    }
}
