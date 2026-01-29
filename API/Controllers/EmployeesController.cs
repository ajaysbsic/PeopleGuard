using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly AppDbContext _context;

    public EmployeesController(IEmployeeService employeeService, AppDbContext context)
    {
        _employeeService = employeeService;
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpGet("by-employee-id/{employeeId}")]
    public async Task<ActionResult<EmployeeDto>> GetByEmployeeId(string employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll([FromQuery] string? query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            // Filter by employee ID or name
            var filtered = await _employeeService.SearchAsync(query, null, null, cancellationToken);
            return Ok(filtered);
        }
        
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return Ok(employees);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> Search([FromQuery] string? name, [FromQuery] string? department, [FromQuery] string? factory, CancellationToken cancellationToken)
    {
        var employees = await _employeeService.SearchAsync(name, department, factory, cancellationToken);
        return Ok(employees);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.UpdateAsync(id, dto, cancellationToken);
        return Ok(employee);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Employee stats: total cases, open, closed, verbal & written warnings
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<EmployeeStatsDto>> GetStats(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _context.Employees.AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        if (!exists) return NotFound(new { message = "Employee not found" });

        var investigations = await _context.Investigations
            .Where(i => i.EmployeeId == id && !i.IsDeleted)
            .Select(i => new { i.Status, i.Outcome })
            .ToListAsync(cancellationToken);

        var warnings = await _context.WarningLetters
            .Where(w => w.EmployeeId == id)
            .Select(w => w.Outcome)
            .ToListAsync(cancellationToken);

        var stats = new EmployeeStatsDto
        {
            TotalCases = investigations.Count,
            Open = investigations.Count(i => i.Status != InvestigationStatus.Closed),
            Closed = investigations.Count(i => i.Status == InvestigationStatus.Closed),
            VerbalWarnings = warnings.Count(w => w == WarningOutcome.VerbalWarning),
            WrittenWarnings = warnings.Count(w => w == WarningOutcome.WrittenWarning)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Employee history (investigations + warnings) with filters and paging
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult<PagedResponse<EmployeeHistoryListItemDto>>> GetHistory(
        Guid id,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Employees.AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        if (!exists) return NotFound(new { message = "Employee not found" });

        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);
        var fromDate = from ?? new DateTime(1753, 1, 1);
        var toDate = to ?? new DateTime(9999, 12, 31);

        var invQuery = _context.Investigations
            .Where(i => i.EmployeeId == id && !i.IsDeleted && i.CreatedAt >= fromDate && i.CreatedAt <= toDate);

        var warnQuery = _context.WarningLetters
            .Where(w => w.EmployeeId == id && w.IssuedAt >= fromDate && w.IssuedAt <= toDate);

        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.ToLowerInvariant();
            if (t == "investigation")
                warnQuery = warnQuery.Where(_ => false); // exclude
            else if (t == "warning")
                invQuery = invQuery.Where(_ => false);
        }

        var invItemsRaw = await invQuery
            .Select(i => new
            {
                i.Id,
                i.Title,
                i.CaseType,
                i.Status,
                i.Outcome,
                i.CreatedAt,
                i.Description
            })
            .ToListAsync(cancellationToken);

        var warnItemsRaw = await warnQuery
            .Select(w => new
            {
                w.Id,
                w.InvestigationId,
                w.Outcome,
                w.IssuedAt,
                w.LetterContent
            })
            .ToListAsync(cancellationToken);

        var invItems = invItemsRaw.Select(i => new EmployeeHistoryListItemDto
        {
            Id = i.Id,
            InvestigationId = i.Id,
            Kind = "Investigation",
            Title = i.Title,
            CaseType = i.CaseType.ToString(),
            Status = i.Status.ToString(),
            Outcome = i.Outcome.HasValue ? i.Outcome.ToString() : null,
            Date = i.CreatedAt,
            Description = i.Description
        });

        var warnItems = warnItemsRaw.Select(w => new EmployeeHistoryListItemDto
        {
            Id = w.Id,
            InvestigationId = w.InvestigationId,
            Kind = "Warning",
            Title = "Warning Letter",
            CaseType = "Warning",
            Status = "Issued",
            Outcome = w.Outcome.ToString(),
            Date = w.IssuedAt,
            Description = w.LetterContent
        });

        var combined = invItems.Concat(warnItems).ToList();

        var total = combined.Count;

        var items = combined
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var response = new PagedResponse<EmployeeHistoryListItemDto>
        {
            Data = items,
            Page = page,
            Size = size,
            Total = total
        };

        return Ok(response);
    }
}
