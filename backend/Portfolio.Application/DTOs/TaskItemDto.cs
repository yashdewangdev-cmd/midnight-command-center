using Portfolio.Domain.Enums;

namespace Portfolio.Application.DTOs;

public class TaskItemDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public double TotalHoursLogged { get; set; }
    public int SessionCount { get; set; }
    public bool HasActiveSession { get; set; }
}
