using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeInvestigationSystem.Infrastructure.SeedData;

public class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await EnsureRolesAsync();
        await EnsureAdminUserAsync();
        await EnsureManagerUserAsync();
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create role {Role}: {Errors}", roleName, errors);
                throw new InvalidOperationException(errors);
            }
        }
    }

    private async Task EnsureAdminUserAsync()
    {
        var adminEmail = _configuration.GetValue<string>("AdminUser:Email");
        var adminPassword = _configuration.GetValue<string>("AdminUser:Password");

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogWarning("Admin user seed skipped due to missing credentials in configuration.");
            return;
        }

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            await EnsureUserInRoleAsync(existingAdmin, UserRole.Admin.ToString());
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
            throw new InvalidOperationException(errors);
        }

        await EnsureUserInRoleAsync(adminUser, UserRole.Admin.ToString());
    }

    private async Task EnsureManagerUserAsync()
    {
        var managerEmail = "manager@alfanar.com";
        var managerPassword = "Ajk@123#";

        var existingManager = await _userManager.FindByEmailAsync(managerEmail);
        if (existingManager is not null)
        {
            await EnsureUserInRoleAsync(existingManager, UserRole.Manager.ToString());
            return;
        }

        var managerUser = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(managerUser, managerPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create manager user: {Errors}", errors);
            throw new InvalidOperationException(errors);
        }

        await EnsureUserInRoleAsync(managerUser, UserRole.Manager.ToString());
    }

    private async Task EnsureUserInRoleAsync(ApplicationUser user, string roleName)
    {
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to assign {Role} role: {Errors}", roleName, errors);
            throw new InvalidOperationException(errors);
        }
    }
}
