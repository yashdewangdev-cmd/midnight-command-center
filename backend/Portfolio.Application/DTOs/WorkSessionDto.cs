using Portfolio.Domain.Enums;

namespace Portfolio.Application.DTOs;

public class WorkSessionDto
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; }
    public string? Notes { get; set; }
    public double DurationHours { get; set; }
}
