namespace EmployeeInvestigationSystem.Domain.Entities;

/// <summary>
/// Stores refresh tokens for JWT token rotation
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    
    // Navigation property
    public virtual ApplicationUser User { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}
