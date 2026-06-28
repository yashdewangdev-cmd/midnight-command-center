using Portfolio.Domain.Enums;

namespace Portfolio.Application.DTOs;

/// <summary>
/// Represents a group of tasks with the same status in a report.
/// </summary>
public class TaskReportGroupDto
{
    public TaskItemStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public double TotalHours { get; set; }
    public List<TaskItemDto> Tasks { get; set; } = new();
}

/// <summary>
/// Full productivity report grouped by task status.
/// </summary>
public class ProductivityReportDto
{
    public Guid UserProfileId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public double TotalHoursLogged { get; set; }
    public int TotalTasksCount { get; set; }
    public int CompletedTasksCount { get; set; }

    public List<TaskReportGroupDto> Groups { get; set; } = new();
}
