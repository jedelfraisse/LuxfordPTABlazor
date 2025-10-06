using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuxfordPTAWeb.Services;

namespace LuxfordPTAWeb.Controllers
{
    /// <summary>
    /// PTA Tech Note: Backup File Controller
    /// 
    /// This controller handles downloading backup files securely.
    /// Only administrators can access backup files.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BackupController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IDatabaseBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        /// <summary>
        /// Download a backup file
        /// </summary>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadBackup(string fileName)
        {
            try
            {
                // Validate file name to prevent directory traversal attacks
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest("Invalid file name");
                }

                var filePath = await _backupService.GetBackupFileAsync(fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Backup file '{fileName}' not found");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                
                _logger.LogInformation("Admin user {User} downloaded backup file {FileName}", 
                    User.Identity?.Name, fileName);

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Backup file not found: {FileName}", fileName);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backup file {FileName}", fileName);
                return StatusCode(500, "Error downloading backup file");
            }
        }

        /// <summary>
        /// Create a new backup
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBackup([FromBody] CreateBackupRequest? request = null)
        {
            try
            {
                var backupFileName = await _backupService.CreateBackupAsync(request?.Name);
                
                _logger.LogInformation("Admin user {User} created backup: {BackupFileName}", 
                    User.Identity?.Name, backupFileName);

                return Ok(new { fileName = backupFileName, message = "Backup created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                return StatusCode(500, new { error = "Error creating backup", details = ex.Message });
            }
        }

        /// <summary>
        /// Get list of available backups
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetBackups()
        {
            try
            {
                var backups = await _backupService.GetBackupListAsync();
                return Ok(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving backup list");
                return StatusCode(500, new { error = "Error retrieving backup list", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a backup file
        /// </summary>
        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteBackup(string fileName)
        {
            try
            {
                // Validate file name to prevent directory traversal attacks
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest("Invalid file name");
                }

                var success = await _backupService.DeleteBackupAsync(fileName);
                
                if (success)
                {
                    _logger.LogInformation("Admin user {User} deleted backup file {FileName}", 
                        User.Identity?.Name, fileName);
                    return Ok(new { message = $"Backup '{fileName}' deleted successfully" });
                }
                else
                {
                    return NotFound($"Backup file '{fileName}' not found or could not be deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup file {FileName}", fileName);
                return StatusCode(500, new { error = "Error deleting backup file", details = ex.Message });
            }
        }
    }

    public class CreateBackupRequest
    {
        public string? Name { get; set; }
    }
}