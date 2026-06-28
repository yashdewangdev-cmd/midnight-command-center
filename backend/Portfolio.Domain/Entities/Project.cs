using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents a portfolio project or a productivity workspace.
/// Contains tasks which in turn track work sessions.
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>
    /// Comma-separated technology tags (e.g., "React, .NET, PostgreSQL").
    /// </summary>
    public string? TechnologyStack { get; set; }

    public string? RepositoryUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public UserProfile UserProfile { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
