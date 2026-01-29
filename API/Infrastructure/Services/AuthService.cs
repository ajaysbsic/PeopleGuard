using EmployeeInvestigationSystem.Application.DTOs.Auth;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:AccessTokenLifetimeMinutes", 60));
        var accessToken = _jwtTokenGenerator.Generate(user, roles, accessTokenExpiresAt);

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue("Jwt:RefreshTokenLifetimeDays", 7));

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshTokenExpiresAt,
            IsRevoked = false
        };
        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            AccessToken = accessToken,
            ExpiresAtUtc = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiryUtc = refreshTokenExpiresAt
        };
    }

    public async Task<RefreshResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (tokenEntity is null || !tokenEntity.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = tokenEntity.User;
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        // Revoke the old token
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAtUtc = DateTime.UtcNow;

        // Generate new tokens (token rotation)
        var roles = await _userManager.GetRolesAsync(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:AccessTokenLifetimeMinutes", 60));
        var accessToken = _jwtTokenGenerator.Generate(user, roles, accessTokenExpiresAt);

        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue("Jwt:RefreshTokenLifetimeDays", 7));

        tokenEntity.ReplacedByToken = newRefreshToken;

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshTokenExpiresAt,
            IsRevoked = false
        };
        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshResponseDto
        {
            AccessToken = accessToken,
            ExpiresAtUtc = accessTokenExpiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiryUtc = refreshTokenExpiresAt
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (tokenEntity is not null && !tokenEntity.IsRevoked)
        {
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<AuthResponseDto> CreateUserAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            IsActive = request.IsActive
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            throw new InvalidOperationException($"Role '{request.Role}' does not exist.");
        }

        var roleAssignResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleAssignResult.Succeeded)
        {
            var errors = string.Join("; ", roleAssignResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:AccessTokenLifetimeMinutes", 60));
        var token = _jwtTokenGenerator.Generate(user, roles, expiresAt);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            AccessToken = token,
            ExpiresAtUtc = expiresAt
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
