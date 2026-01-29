using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Manages warning letters issued to employees.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarningLettersController : ControllerBase
{
    private readonly IWarningLetterService _warningLetterService;
    private readonly ILogger<WarningLettersController> _logger;

    public WarningLettersController(IWarningLetterService warningLetterService, ILogger<WarningLettersController> logger)
    {
        _warningLetterService = warningLetterService;
        _logger = logger;
    }

    /// <summary>
    /// Get all warning letters.
    /// </summary>
    /// <returns>List of warning letters</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WarningLetterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var letters = await _warningLetterService.GetAllAsync(cancellationToken);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving warning letters: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve warning letters" });
        }
    }

    /// <summary>
    /// Create a warning letter for an investigation.
    /// </summary>
    /// <param name="investigationId">ID of the investigation</param>
    /// <param name="request">Warning letter creation request</param>
    /// <returns>Created warning letter details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Business,ER,Management,Manager")]
    [ProducesResponseType(typeof(WarningLetterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateWarningLetter([FromBody] CreateWarningLetterRequest request)
    {
        try
        {
            var warningLetter = await _warningLetterService.CreateWarningLetterAsync(
                request.InvestigationId,
                request.Outcome,
                request.EmployeeId,
                request.Reason);

            _logger.LogInformation("Warning letter created for investigation {InvestigationId}", request.InvestigationId);
            return CreatedAtAction(nameof(GetWarningLetterByInvestigation), new { investigationId = request.InvestigationId }, warningLetter);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Warning letter creation failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating warning letter: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to create warning letter" });
        }
    }

    /// <summary>
    /// Get warning letter by investigation ID.
    /// </summary>
    /// <param name="investigationId">ID of the investigation</param>
    /// <returns>Warning letter details</returns>
    [HttpGet("by-investigation/{investigationId}")]
    [ProducesResponseType(typeof(WarningLetterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarningLetterByInvestigation(Guid investigationId)
    {
        try
        {
            var warningLetter = await _warningLetterService.GetByInvestigationIdAsync(investigationId);
            if (warningLetter == null)
            {
                return NotFound(new { message = $"Warning letter for investigation {investigationId} not found" });
            }

            return Ok(warningLetter);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving warning letter: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve warning letter" });
        }
    }

    /// <summary>
    /// Download warning letter PDF.
    /// </summary>
    /// <param name="warningLetterId">ID of the warning letter</param>
    /// <returns>PDF file</returns>
    [HttpGet("{warningLetterId}/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarningLetterPdf(Guid warningLetterId)
    {
        try
        {
            var pdfBytes = await _warningLetterService.GetWarningLetterPdfAsync(warningLetterId);
            if (pdfBytes.Length == 0)
            {
                return NotFound(new { message = "PDF not found" });
            }

            return File(pdfBytes, "application/pdf", $"warning_letter_{warningLetterId}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Warning letter PDF not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving warning letter PDF: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve warning letter PDF" });
        }
    }
}

/// <summary>
/// Request to create a warning letter.
/// </summary>
public class CreateWarningLetterRequest
{
    /// <summary>
    /// ID of the investigation.
    /// </summary>
    public Guid InvestigationId { get; set; }

    /// <summary>
    /// ID of the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Warning outcome.
    /// </summary>
    public WarningOutcome Outcome { get; set; }

    /// <summary>
    /// Reason for the warning.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
