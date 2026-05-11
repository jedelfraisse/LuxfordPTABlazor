using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BoardMember")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadController> _logger;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private const string AllowedImageExtensions = ".jpg,.jpeg,.png,.gif,.webp";
    private const string AllowedPdfExtensions = ".pdf";

    public FileUploadController(IWebHostEnvironment env, ILogger<FileUploadController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Upload an image file (program images, logos, etc.)
    /// </summary>
    [HttpPost("upload-image")]
    public async Task<ActionResult<string>> UploadImage([FromForm] IFormFile file, [FromForm] string folder = "programs")
    {
        return await UploadFile(file, "images", folder);
    }

    /// <summary>
    /// Upload a PDF file (flyers, documents, etc.)
    /// </summary>
    [HttpPost("upload-pdf")]
    public async Task<ActionResult<string>> UploadPdf([FromForm] IFormFile file)
    {
        return await UploadFile(file, "flyers", null);
    }

    private async Task<ActionResult<string>> UploadFile(IFormFile file, string baseFolder, string? subfolder)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file selected" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { error = $"File size exceeds {MaxFileSize / 1024 / 1024}MB limit" });
            }

            // Determine allowed extensions based on folder
            var allowedExtensions = baseFolder == "images" ? AllowedImageExtensions : AllowedPdfExtensions;
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { error = $"File type not allowed. Allowed types: {allowedExtensions}" });
            }

            // Sanitize filename
            var fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName)) + fileExtension;

            // Create directory path
            var uploadPath = Path.Combine(_env.WebRootPath, baseFolder, subfolder ?? "");
            Directory.CreateDirectory(uploadPath);

            // Handle file name collision
            var filePath = Path.Combine(uploadPath, fileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var counter = 1;

            while (System.IO.File.Exists(filePath))
            {
                fileName = $"{fileNameWithoutExt}_{counter}{fileExtension}";
                filePath = Path.Combine(uploadPath, fileName);
                counter++;
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path
            var relativePath = Path.Combine(baseFolder, subfolder ?? "", fileName)
                .Replace("\\", "/");

            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);

            return Ok(new { url = relativePath, fileName = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while uploading the file" });
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        // Replace spaces with underscores
        fileName = fileName.Replace(" ", "_");

        // Limit length
        if (fileName.Length > 100)
        {
            fileName = fileName.Substring(0, 100);
        }

        return fileName.ToLowerInvariant();
    }

    /// <summary>
    /// Delete an uploaded file
    /// </summary>
    [HttpDelete("delete")]
    public ActionResult DeleteFile([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return BadRequest(new { error = "File path is required" });
            }

            // Security: Prevent directory traversal attacks
            if (filePath.Contains(".."))
            {
                return BadRequest(new { error = "Invalid file path" });
            }

            var fullPath = Path.Combine(_env.WebRootPath, filePath);
            var wwwRootPath = Path.GetFullPath(_env.WebRootPath);
            var fullPathResolved = Path.GetFullPath(fullPath);

            // Ensure file is within wwwroot
            if (!fullPathResolved.StartsWith(wwwRootPath))
            {
                return BadRequest(new { error = "Invalid file path" });
            }

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Ok(new { message = "File deleted successfully" });
            }

            return NotFound(new { error = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while deleting the file" });
        }
    }
}
