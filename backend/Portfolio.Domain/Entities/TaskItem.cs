using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents a trackable task within a project.
/// Named TaskItem to avoid collision with System.Threading.Tasks.Task.
/// Supports multiple work sessions for cumulative time tracking.
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Optional due date for the task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();

    /// <summary>
    /// Calculates total time spent across all work sessions for this task.
    /// </summary>
    public TimeSpan TotalTimeSpent => WorkSessions
        .Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);
}
