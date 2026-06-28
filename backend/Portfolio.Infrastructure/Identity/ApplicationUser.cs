using Microsoft.AspNetCore.Identity;

namespace Portfolio.Infrastructure.Identity;

/// <summary>
/// Extends IdentityUser with application-specific fields.
/// Linked to UserProfile for portfolio data via UserProfileId.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Links this Identity user to the domain UserProfile entity.
    /// </summary>
    public Guid? UserProfileId { get; set; }

    /// <summary>
    /// Stores the refresh token for JWT token rotation.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Expiration time of the current refresh token.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
