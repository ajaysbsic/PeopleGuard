namespace EmployeeInvestigationSystem.Application.DTOs.Auth;

public record RegisterUserRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
