using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Public QR/Barcode submission endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QrController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<QrController> _logger;
    private readonly IConfiguration _config;

    public QrController(AppDbContext context, ILogger<QrController> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Generate a new QR token (Admin only)
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<QrTokenResponse>> GenerateToken(
        [FromBody] GenerateQrTokenRequest request,
        CancellationToken cancellationToken)
    {
        var token = GenerateSecureToken();
        var expiryDays = int.TryParse(_config["Qr:ExpiryDays"], out var days) ? days : 30;

        var qrToken = new Domain.Entities.QrToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            TargetType = request.TargetType ?? "general",
            TargetId = request.TargetId ?? "general",
            Label = request.Label,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsActive = true,
            CreatedBy = GetCurrentUserId()
        };

        _context.QrTokens.Add(qrToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate QR code
        var (qrImageUrl, qrPngPath) = GenerateQrCode(qrToken.Id, token);
        qrToken.QrPngPath = qrPngPath;

        _logger.LogInformation("QR token generated: {TokenId} by {User}", qrToken.Id, GetCurrentUserName());

        return Ok(new QrTokenResponse
        {
            TokenId = qrToken.Id,
            Token = token,
            QrImageUrl = qrImageUrl,
            ExpiresAt = qrToken.ExpiresAt
        });
    }

    /// <summary>
    /// Get QR token details for display/download
    /// </summary>
    [HttpGet("{tokenId:guid}")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> GetToken(Guid tokenId, CancellationToken cancellationToken)
    {
        var qrToken = await _context.QrTokens
            .Where(q => q.Id == tokenId)
            .FirstOrDefaultAsync(cancellationToken);

        if (qrToken == null)
            return NotFound(new { message = "QR token not found" });

        return Ok(new QrTokenResponse
        {
            TokenId = qrToken.Id,
            Token = qrToken.Token,
            QrImageUrl = $"/api/qr/{qrToken.Id}/image",
            ExpiresAt = qrToken.ExpiresAt
        });
    }

    /// <summary>
    /// Download QR image
    /// </summary>
    [HttpGet("{tokenId:guid}/image")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> GetQrImage(Guid tokenId, CancellationToken cancellationToken)
    {
        var qrToken = await _context.QrTokens
            .Where(q => q.Id == tokenId && q.IsActive && q.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (qrToken == null)
            return NotFound();

        var path = !string.IsNullOrWhiteSpace(qrToken.QrPngPath)
            ? qrToken.QrPngPath
            : Path.Combine(Directory.GetCurrentDirectory(), "uploads", "qr", $"{tokenId}.png");

        if (!System.IO.File.Exists(path))
            return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(path, cancellationToken);
        return File(bytes, "image/png", $"qr-{qrToken.Label ?? qrToken.TargetId}.png");
    }

    /// <summary>
    /// Public: Submit complaint/suggestion via QR (no auth required if allowAnonymous)
    /// </summary>
    [HttpPost("submit")]
    [AllowAnonymous]
    public async Task<ActionResult<QrSubmissionResponse>> SubmitComplaint(
        [FromBody] QrSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var allowAnonymous = bool.TryParse(_config["Qr:AllowAnonymous"], out var allow) ? allow : true;
        if (!allowAnonymous)
            return Unauthorized();

        // Validate token
        var qrToken = await _context.QrTokens
            .Where(q => q.Token == request.Token && q.IsActive && q.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (qrToken == null)
            return BadRequest(new { message = "Invalid or expired token" });

        // Create submission
        var submission = new Domain.Entities.QrSubmission
        {
            Id = Guid.NewGuid(),
            TokenId = qrToken.Id,
            Category = request.Category ?? "complaint",
            Message = request.Message,
            SubmitterName = request.SubmitterName,
            SubmitterEmail = request.SubmitterEmail,
            SubmitterPhone = request.SubmitterPhone,
            AttachmentUrls = request.AttachmentUrls != null ? string.Join(",", request.AttachmentUrls) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.QrSubmissions.Add(submission);

        // Create related investigation case
        var caseTitle = $"[{request.Category.ToUpper()}] {request.SubmitterName ?? "Anonymous"} - {qrToken.Label ?? qrToken.TargetId}";
        var investigation = new Domain.Entities.Investigation
        {
            Id = Guid.NewGuid(),
            EmployeeId = await GetOrCreateAnonymousEmployee(cancellationToken),
            Title = caseTitle,
            Description = request.Message,
            CaseType = Domain.Enums.InvestigationCaseType.Complaint,
            Status = Domain.Enums.InvestigationStatus.Open,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Investigations.Add(investigation);
        submission.RelatedInvestigationId = investigation.Id;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("QR submission received from {Submitter} via token {TokenId}", 
            request.SubmitterEmail ?? "Anonymous", qrToken.Id);

        return Ok(new QrSubmissionResponse
        {
            SubmissionId = submission.Id,
            CaseId = investigation.Id,
            Message = "Thank you for your submission. We will review it promptly."
        });
    }

    /// <summary>
    /// List QR tokens (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> ListTokens([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        var total = await _context.QrTokens.CountAsync(cancellationToken);
        var tokens = await _context.QrTokens
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(q => new
            {
                q.Id,
                q.Token,
                q.TargetType,
                q.TargetId,
                q.Label,
                q.IsActive,
                q.ExpiresAt,
                q.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            data = tokens,
            page,
            size,
            total,
            totalPages = Math.Ceiling((double)total / size)
        });
    }

    private string GenerateSecureToken()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }

    private (string url, string filePath) GenerateQrCode(Guid tokenId, string token)
    {
        var qrUrl = $"{Request.Scheme}://{Request.Host}/qr/{token}";
        var generator = new QRCodeGenerator();
        var qrCodeData = generator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);

        var dirPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "qr");
        Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, $"{tokenId}.png");
        System.IO.File.WriteAllBytes(filePath, qrCodeImage);

        return ($"/api/qr/{tokenId}/image", filePath);
    }

    private async Task<Guid> GetOrCreateAnonymousEmployee(CancellationToken cancellationToken)
    {
        var anonymous = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == "ANONYMOUS", cancellationToken);

        if (anonymous != null)
            return anonymous.Id;

        var newAnon = new Domain.Entities.Employee
        {
            Id = Guid.NewGuid(),
            EmployeeId = "ANONYMOUS",
            Name = "Anonymous Submitter",
            Department = "Public",
            Factory = "External",
            Designation = "N/A",
            IsDeleted = false
        };

        _context.Employees.Add(newAnon);
        await _context.SaveChangesAsync(cancellationToken);
        return newAnon.Id;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User.Identity?.Name ?? "Unknown";
    }
}
