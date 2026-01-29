using EmployeeInvestigationSystem.Application.DTOs.Auth;

namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> CreateUserAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default);
    Task<RefreshResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
