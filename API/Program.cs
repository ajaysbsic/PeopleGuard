using EmployeeInvestigationSystem.API.Middleware;
using EmployeeInvestigationSystem.Application;
using EmployeeInvestigationSystem.Infrastructure;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using EmployeeInvestigationSystem.Infrastructure.SeedData;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var supportedCultures = builder.Configuration.GetSection("Localization:SupportedCultures").Get<string[]>() ?? new[] { "en" };
var defaultCulture = builder.Configuration.GetValue<string>("Localization:DefaultCulture") ?? "en";

builder.Services
    .AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    
    if (allowedOrigins.Contains("*"))
    {
        // Allow all origins
        options.AddPolicy("DefaultCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // Restrict to specific origins
        options.AddPolicy("DefaultCors", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedAsync();

        var employeeSeeder = scope.ServiceProvider.GetRequiredService<EmployeeSeeder>();
        var seedPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "SeedData", "employees.json");
        await employeeSeeder.SeedAsync(seedPath);

        var investigationSeeder = scope.ServiceProvider.GetRequiredService<InvestigationSeeder>();
        await investigationSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during database seeding");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

// CORS must be applied BEFORE authentication and authorization
app.UseCors("DefaultCors");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(defaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuditLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
