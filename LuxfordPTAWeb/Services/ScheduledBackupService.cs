using Microsoft.Extensions.Options;

namespace LuxfordPTAWeb.Services
{
    /// <summary>
    /// PTA Tech Note: Scheduled Backup Service
    /// 
    /// This service runs daily backups automatically in the background.
    /// It's configured to run at 2 AM daily by default.
    /// 
    /// Configuration in appsettings.json:
    /// "BackupSettings": {
    ///   "EnableDailyBackup": true,
    ///   "BackupTime": "02:00",
    ///   "RetentionDays": 30
    /// }
    /// </summary>
    public class ScheduledBackupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledBackupService> _logger;
        private readonly BackupSettings _settings;

        public ScheduledBackupService(
            IServiceProvider serviceProvider,
            ILogger<ScheduledBackupService> logger,
            IOptions<BackupSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.EnableDailyBackup)
            {
                _logger.LogInformation("Daily backups are disabled in configuration");
                return;
            }

            _logger.LogInformation("Scheduled backup service started. Daily backup time: {BackupTime}", _settings.BackupTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var targetTime = DateTime.Today.Add(_settings.BackupTimeSpan);

                    // If target time has passed today, schedule for tomorrow
                    if (now > targetTime)
                    {
                        targetTime = targetTime.AddDays(1);
                    }

                    var delay = targetTime - now;
                    _logger.LogInformation("Next backup scheduled for: {TargetTime} (in {DelayHours:F1} hours)", 
                        targetTime, delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformScheduledBackupAsync();
                        
                        // Clean up old backups
                        await CleanupOldBackupsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scheduled backup service");
                    
                    // Wait 1 hour before retrying if there's an error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task PerformScheduledBackupAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();

                var backupName = $"Daily_Backup_{DateTime.Now:yyyyMMdd}";
                var backupFileName = await backupService.CreateBackupAsync(backupName);

                _logger.LogInformation("Scheduled backup completed successfully: {BackupFileName}", backupFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform scheduled backup");
            }
        }

        private async Task CleanupOldBackupsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();

                var backups = await backupService.GetBackupListAsync();
                var cutoffDate = DateTime.Now.AddDays(-_settings.RetentionDays);

                var oldBackups = backups.Where(b => b.CreatedDate < cutoffDate && b.FileName.Contains("Daily_Backup")).ToList();

                foreach (var oldBackup in oldBackups)
                {
                    var deleted = await backupService.DeleteBackupAsync(oldBackup.FileName);
                    if (deleted)
                    {
                        _logger.LogInformation("Deleted old backup: {BackupFileName} (created {CreatedDate})", 
                            oldBackup.FileName, oldBackup.CreatedDate);
                    }
                }

                if (oldBackups.Any())
                {
                    _logger.LogInformation("Cleanup completed. Removed {Count} old backups", oldBackups.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old backups");
            }
        }
    }

    /// <summary>
    /// Configuration settings for database backups
    /// </summary>
    public class BackupSettings
    {
        public const string SectionName = "BackupSettings";

        public bool EnableDailyBackup { get; set; } = true;
        public string BackupTime { get; set; } = "02:00";
        public int RetentionDays { get; set; } = 30;

        public TimeSpan BackupTimeSpan => TimeSpan.Parse(BackupTime);
    }
}