using Microsoft.AspNetCore.Identity;

namespace EmployeeInvestigationSystem.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    
    // Navigation property for refresh tokens
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
