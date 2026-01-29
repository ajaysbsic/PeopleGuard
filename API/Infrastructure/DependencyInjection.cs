using System.Text;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Application.Services;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using EmployeeInvestigationSystem.Infrastructure.Repositories;
using EmployeeInvestigationSystem.Infrastructure.SeedData;
using EmployeeInvestigationSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeInvestigationSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>();

        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var jwtAudience = jwtSection.GetValue<string>("Audience") ?? string.Empty;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IdentitySeeder>();
        
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeHistoryService, EmployeeHistoryService>();
        services.AddScoped<EmployeeSeeder>();

        services.AddScoped<IInvestigationRepository, InvestigationRepository>();
        services.AddScoped<IInvestigationService, InvestigationService>();
        services.AddScoped<InvestigationSeeder>();

        services.AddScoped<IQRCodeService, QRCodeService>();
        services.AddScoped<IPdfGenerationService, PdfGenerationService>();
        services.AddScoped<IWarningLetterService, WarningLetterService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
