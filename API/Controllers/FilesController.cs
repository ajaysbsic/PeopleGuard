using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FilesController> _logger;
    
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

    public FilesController(IWebHostEnvironment env, ILogger<FilesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// POST /api/files
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<FileUploadResult>> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate file size
        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = $"File size exceeds {MaxFileSizeBytes / (1024 * 1024)}MB limit" });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"File type not allowed: {extension}" });
        }

        try
        {
            // Generate unique filename
            var fileId = Guid.NewGuid().ToString();
            var fileName = $"{fileId}{extension}";
            
            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Don't log PII - just log file ID
            _logger.LogInformation("File uploaded: {FileId}", fileId);

            return Ok(new FileUploadResult
            {
                FileId = fileId,
                Url = $"/api/files/{fileId}",
                FileName = file.FileName,
                Size = file.Length,
                ContentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed");
            return StatusCode(500, new { message = "File upload failed" });
        }
    }

    /// <summary>
    /// Get a file by ID
    /// GET /api/files/{fileId}
    /// </summary>
    [HttpGet("{fileId}")]
    public IActionResult GetFile(string fileId)
    {
        var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
        
        // Find file with any extension
        var files = Directory.GetFiles(uploadsFolder, $"{fileId}.*");
        if (files.Length == 0)
        {
            return NotFound(new { message = "File not found" });
        }

        var filePath = files[0];
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = GetContentType(extension);

        return PhysicalFile(filePath, contentType);
    }

    /// <summary>
    /// Delete a file
    /// DELETE /api/files/{fileId}
    /// </summary>
    [HttpDelete("{fileId}")]
    public IActionResult DeleteFile(string fileId)
    {
        var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
        
        // Find file with any extension
        var files = Directory.GetFiles(uploadsFolder, $"{fileId}.*");
        if (files.Length == 0)
        {
            return NotFound(new { message = "File not found" });
        }

        try
        {
            System.IO.File.Delete(files[0]);
            _logger.LogInformation("File deleted: {FileId}", fileId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File deletion failed: {FileId}", fileId);
            return StatusCode(500, new { message = "File deletion failed" });
        }
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}

public record FileUploadResult
{
    public string FileId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }
    public string ContentType { get; init; } = string.Empty;
}
