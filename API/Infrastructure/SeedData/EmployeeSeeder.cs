using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EmployeeInvestigationSystem.Infrastructure.SeedData;

public class EmployeeSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeSeeder> _logger;

    public EmployeeSeeder(AppDbContext context, ILogger<EmployeeSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(string seedFilePath)
    {
        if (_context.Set<Employee>().Any())
        {
            _logger.LogInformation("Employees already seeded, skipping.");
            return;
        }

        if (!File.Exists(seedFilePath))
        {
            _logger.LogWarning("Employee seed file not found at {Path}", seedFilePath);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(seedFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var employees = JsonSerializer.Deserialize<List<EmployeeSeedData>>(json, options) ?? new List<EmployeeSeedData>();

            var seedEmployees = employees.Select(e => new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Department = e.Department,
                Factory = e.Factory,
                Designation = e.Designation,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Set<Employee>().AddRangeAsync(seedEmployees);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} employees", seedEmployees.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding employees");
            throw;
        }
    }

    private class EmployeeSeedData
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Factory { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }
}
