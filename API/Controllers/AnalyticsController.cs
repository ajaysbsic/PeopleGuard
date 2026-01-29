using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Lightweight analytics endpoints for violations breakdowns and drill-through.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Violations grouped by factory | department | type.
    /// </summary>
    [HttpGet("violations")]
    public async Task<IActionResult> GetViolations(
        [FromQuery] string groupBy = "factory",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var fromDate = from ?? DateTime.MinValue;
        var toDate = to ?? DateTime.MaxValue;

        var query = _context.Investigations
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.CreatedAt >= fromDate && i.CreatedAt <= toDate);

        IQueryable<ChartDataDto> grouped = groupBy.ToLowerInvariant() switch
        {
            "department" => query
                .GroupBy(i => i.Employee!.Department)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key ?? "Unknown",
                    Value = g.Count(),
                    Percentage = 0m
                }),
            "type" => query
                .GroupBy(i => i.CaseType)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key.ToString(),
                    Value = g.Count(),
                    Percentage = 0m
                }),
            _ => query
                .GroupBy(i => i.Employee!.Factory)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key ?? "Unknown",
                    Value = g.Count(),
                    Percentage = 0m
                })
        };

        var data = await grouped
            .OrderByDescending(x => x.Value)
            .ToListAsync(cancellationToken);

        // Compute percentage after materialization
        var total = data.Sum(d => d.Value);
        if (total > 0)
        {
            foreach (var item in data)
            {
                item.Percentage = Math.Round((decimal)item.Value / total * 100, 2);
            }
        }

        return Ok(new
        {
            labels = data.Select(d => d.Label).ToArray(),
            values = data.Select(d => d.Value).ToArray(),
            items = data
        });
    }
}
