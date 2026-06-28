using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents a user's public-facing portfolio profile.
/// Linked to ApplicationUser (Identity) via a shared UserId.
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the ASP.NET Identity ApplicationUser.
    /// </summary>
    public string IdentityUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
