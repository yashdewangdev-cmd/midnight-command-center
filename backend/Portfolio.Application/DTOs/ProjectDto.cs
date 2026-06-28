using Portfolio.Domain.Enums;

namespace Portfolio.Application.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public string? TechnologyStack { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TaskCount { get; set; }
    public double TotalHoursLogged { get; set; }
}
