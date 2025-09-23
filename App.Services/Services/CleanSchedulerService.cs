using App.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.Services;

public class CleanSchedulerService : BackgroundService
{
    private readonly ILogger<CleanSchedulerService> _logger;
    private readonly IArchiveAndCleanupManager _archiveAndCleanupManager;
    private readonly int _intervalMinutes;

    public CleanSchedulerService(
        ILogger<CleanSchedulerService> logger,
        IArchiveAndCleanupManager archiveAndCleanupManager)
    {
        _logger = logger;
        _archiveAndCleanupManager = archiveAndCleanupManager;
        _intervalMinutes = 60;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CleanSchedulerService started. Interval: {Interval} minutes", _intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting archive cleanup at {Time}", DateTime.UtcNow);

                await _archiveAndCleanupManager.CleanupOldArchivedProductsAsync();

                _logger.LogInformation("Archive cleanup finished at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled archive cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }
}