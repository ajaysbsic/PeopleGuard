using EmployeeInvestigationSystem.Application.DTOs.Auth;
using EmployeeInvestigationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string RefreshTokenCookieName = "pg_refresh_token";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        
        // Set refresh token as httpOnly cookie (OWASP best practice)
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryUtc);
        }
        
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshResponseDto>> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token not found" });
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
            
            // Update refresh token cookie if a new one is issued
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryUtc);
            }
            
            return Ok(new RefreshResponseDto
            {
                AccessToken = result.AccessToken,
                ExpiresAtUtc = result.ExpiresAtUtc
            });
        }
        catch (UnauthorizedAccessException)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        }
        
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    [HttpPost("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponseDto>> CreateUser([FromBody] RegisterUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateUser), new { result.UserId }, result);
    }

    private void SetRefreshTokenCookie(string token, DateTime expiryUtc)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only send over HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = expiryUtc
        };
        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    }
}
