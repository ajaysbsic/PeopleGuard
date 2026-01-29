using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeInvestigationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestigationsController : ControllerBase
{
    private readonly IInvestigationService _investigationService;

    public InvestigationsController(IInvestigationService investigationService)
    {
        _investigationService = investigationService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvestigationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var investigation = await _investigationService.GetByIdAsync(id, cancellationToken);
        if (investigation == null)
            return NotFound();

        return Ok(investigation);
    }

    [HttpGet("by-employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<InvestigationDto>>> GetByEmployeeId(Guid employeeId, CancellationToken cancellationToken)
    {
        var investigations = await _investigationService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return Ok(investigations);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvestigationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var investigations = await _investigationService.GetAllAsync(cancellationToken);
        return Ok(investigations);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<InvestigationDto>>> GetByStatus(InvestigationStatus status, CancellationToken cancellationToken)
    {
        var investigations = await _investigationService.GetByStatusAsync(status, cancellationToken);
        return Ok(investigations);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<InvestigationDto>> Create([FromBody] CreateInvestigationDto dto, CancellationToken cancellationToken)
    {
        var investigation = await _investigationService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = investigation.Id }, investigation);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<InvestigationDto>> Update(Guid id, [FromBody] UpdateInvestigationDto dto, CancellationToken cancellationToken)
    {
        var investigation = await _investigationService.UpdateAsync(id, dto, cancellationToken);
        return Ok(investigation);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,ER")]
    public async Task<ActionResult<InvestigationDto>> ChangeStatus(Guid id, [FromBody] ChangeInvestigationStatusDto dto, CancellationToken cancellationToken)
    {
        var investigation = await _investigationService.ChangeStatusAsync(id, dto.Status, cancellationToken);
        return Ok(investigation);
    }

    [HttpPost("{investigationId}/remarks")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> AddRemark(Guid investigationId, [FromBody] CreateInvestigationRemarkDto dto, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        await _investigationService.AddRemarkAsync(investigationId, dto, userId, cancellationToken);
        return Ok(new { message = "Remark added successfully" });
    }

    [HttpGet("{investigationId}/remarks")]
    public async Task<ActionResult<IEnumerable<InvestigationRemarkDto>>> GetRemarks(Guid investigationId, CancellationToken cancellationToken)
    {
        var remarks = await _investigationService.GetRemarksAsync(investigationId, cancellationToken);
        return Ok(remarks);
    }

    [HttpGet("{investigationId}/attachments")]
    public async Task<ActionResult<IEnumerable<InvestigationAttachmentDto>>> GetAttachments(Guid investigationId, CancellationToken cancellationToken)
    {
        var attachments = await _investigationService.GetAttachmentsAsync(investigationId, cancellationToken);
        return Ok(attachments);
    }
}
