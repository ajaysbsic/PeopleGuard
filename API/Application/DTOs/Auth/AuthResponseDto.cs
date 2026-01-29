namespace EmployeeInvestigationSystem.Application.DTOs.Auth;

using System.Text.Json.Serialization;

public record AuthResponseDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }
    
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;
    
    [JsonPropertyName("roles")]
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;
    
    [JsonPropertyName("expiresAtUtc")]
    public DateTime ExpiresAtUtc { get; init; }
    
    // Refresh token is set via httpOnly cookie, not returned in response body
    [JsonIgnore]
    public string? RefreshToken { get; init; }
    
    [JsonIgnore]
    public DateTime RefreshTokenExpiryUtc { get; init; }
}

public record RefreshResponseDto
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;
    
    [JsonPropertyName("expiresAtUtc")]
    public DateTime ExpiresAtUtc { get; init; }
    
    // Used internally only
    [JsonIgnore]
    public string? RefreshToken { get; init; }
    
    [JsonIgnore]
    public DateTime RefreshTokenExpiryUtc { get; init; }
}
