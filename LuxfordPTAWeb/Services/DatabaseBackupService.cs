using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LuxfordPTAWeb.Data;

namespace LuxfordPTAWeb.Services
{
    /// <summary>
    /// PTA Tech Note: Database Backup Service
    /// 
    /// This service provides automated and on-demand database backups.
    /// Backups are stored locally and can be configured to run daily.
    /// 
    /// For future PTA tech leads:
    /// - Backups are stored in the wwwroot/backups folder
    /// - Daily backups run automatically if enabled in configuration
    /// - Manual backups can be triggered through the admin interface
    /// - Consider setting up cloud storage backup for additional security
    /// </summary>
    public interface IDatabaseBackupService
    {
        Task<string> CreateBackupAsync(string? customName = null);
        Task<List<BackupInfo>> GetBackupListAsync();
        Task<bool> DeleteBackupAsync(string backupFileName);
        Task<string> GetBackupFileAsync(string backupFileName);
    }

    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly string _backupDirectory;

        public DatabaseBackupService(
            IConfiguration configuration, 
            IWebHostEnvironment environment,
            ILogger<DatabaseBackupService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            
            // Create backups directory in wwwroot
            _backupDirectory = Path.Combine(_environment.WebRootPath, "backups");
            Directory.CreateDirectory(_backupDirectory);
        }

        /// <summary>
        /// Creates a database backup with optional custom naming
        /// </summary>
        public async Task<string> CreateBackupAsync(string? customName = null)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupName = customName ?? $"LuxfordPTA_Backup_{timestamp}";
                var backupFileName = $"{backupName}.bak";
                var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Database connection string not found");
                }

                // Extract database name from connection string
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                // Create SQL Server backup command
                var backupSql = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = @backupPath 
                    WITH FORMAT, INIT, 
                    NAME = '{backupName}', 
                    COMPRESSION,
                    DESCRIPTION = 'Luxford PTA Database Backup - {DateTime.Now:yyyy-MM-dd HH:mm:ss}'";

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(backupSql, connection);
                command.Parameters.AddWithValue("@backupPath", backupFilePath);
                command.CommandTimeout = 300; // 5 minutes timeout for large databases

                _logger.LogInformation("Starting database backup to {BackupPath}", backupFilePath);
                
                await command.ExecuteNonQueryAsync();
                
                _logger.LogInformation("Database backup completed successfully: {BackupFileName}", backupFileName);

                // Create metadata file
                await CreateBackupMetadataAsync(backupFileName, backupName);

                return backupFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database backup");
                throw new InvalidOperationException($"Backup failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets list of all available backups with their metadata
        /// </summary>
        public async Task<List<BackupInfo>> GetBackupListAsync()
        {
            var backups = new List<BackupInfo>();

            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, "*.bak");
                
                foreach (var backupFile in backupFiles)
                {
                    var fileName = Path.GetFileName(backupFile);
                    var fileInfo = new FileInfo(backupFile);
                    
                    var backup = new BackupInfo
                    {
                        FileName = fileName,
                        CreatedDate = fileInfo.CreationTime,
                        SizeBytes = fileInfo.Length,
                        SizeFormatted = FormatFileSize(fileInfo.Length)
                    };

                    // Try to load metadata
                    var metadataFile = Path.Combine(_backupDirectory, $"{Path.GetFileNameWithoutExtension(fileName)}.json");
                    if (File.Exists(metadataFile))
                    {
                        try
                        {
                            var metadataJson = await File.ReadAllTextAsync(metadataFile);
                            var metadata = System.Text.Json.JsonSerializer.Deserialize<BackupMetadata>(metadataJson);
                            if (metadata != null)
                            {
                                backup.DisplayName = metadata.DisplayName;
                                backup.Description = metadata.Description;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to read backup metadata for {FileName}", fileName);
                        }
                    }

                    backups.Add(backup);
                }

                return backups.OrderByDescending(b => b.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup list");
                return new List<BackupInfo>();
            }
        }

        /// <summary>
        /// Deletes a backup file and its metadata
        /// </summary>
        public Task<bool> DeleteBackupAsync(string backupFileName)
        {
            try
            {
                var backupFilePath = Path.Combine(_backupDirectory, backupFileName);
                var metadataFilePath = Path.Combine(_backupDirectory, $"{Path.GetFileNameWithoutExtension(backupFileName)}.json");

                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                }

                if (File.Exists(metadataFilePath))
                {
                    File.Delete(metadataFilePath);
                }

                _logger.LogInformation("Deleted backup: {BackupFileName}", backupFileName);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete backup {BackupFileName}", backupFileName);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Gets the full path to a backup file for download
        /// </summary>
        public Task<string> GetBackupFileAsync(string backupFileName)
        {
            var backupFilePath = Path.Combine(_backupDirectory, backupFileName);
            
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupFileName}");
            }

            return Task.FromResult(backupFilePath);
        }

        private Task CreateBackupMetadataAsync(string backupFileName, string displayName)
        {
            try
            {
                var metadata = new BackupMetadata
                {
                    FileName = backupFileName,
                    DisplayName = displayName,
                    CreatedDate = DateTime.Now,
                    Description = $"Automated backup created on {DateTime.Now:yyyy-MM-dd} at {DateTime.Now:HH:mm:ss}",
                    Version = "1.0"
                };

                var metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var metadataFileName = $"{Path.GetFileNameWithoutExtension(backupFileName)}.json";
                var metadataFilePath = Path.Combine(_backupDirectory, metadataFileName);

                return File.WriteAllTextAsync(metadataFilePath, metadataJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create backup metadata for {BackupFileName}", backupFileName);
                return Task.CompletedTask;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Information about a database backup
    /// </summary>
    public class BackupInfo
    {
        public string FileName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public long SizeBytes { get; set; }
        public string SizeFormatted { get; set; } = "";
    }

    /// <summary>
    /// Metadata stored with each backup
    /// </summary>
    public class BackupMetadata
    {
        public string FileName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string Version { get; set; } = "";
    }
}