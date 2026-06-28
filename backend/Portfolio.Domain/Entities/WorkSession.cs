using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// Represents a single contiguous work period on a task.
/// Multiple sessions per task enable disconnected tracking with
/// accurate cumulative duration calculation.
/// </summary>
public class WorkSession
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    /// <summary>
    /// Optional notes recorded during or after the work session.
    /// </summary>
    public string? Notes { get; set; }

    // Navigation
    public TaskItem TaskItem { get; set; } = null!;

    /// <summary>
    /// Calculates the duration of this session.
    /// Active sessions use UtcNow as a provisional end time.
    /// Paused sessions return the elapsed time up to the pause point (EndTime).
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            if (Status == SessionStatus.Active)
                return DateTime.UtcNow - StartTime;

            return EndTime.HasValue
                ? EndTime.Value - StartTime
                : TimeSpan.Zero;
        }
    }
}
