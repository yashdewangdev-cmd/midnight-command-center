using FluentAssertions;
using Moq;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Tests;

/// <summary>
/// Unit tests for the ReportService.
/// Validates task grouping by status and aggregated hours calculation.
/// </summary>
public class ReportServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IWorkSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IUserProfileRepository> _profileRepoMock = new();
    private readonly WorkSessionCalculator _calculator = new();
    private readonly ReportService _service;

    private readonly UserProfile _testUser = new()
    {
        Id = Guid.NewGuid(),
        IdentityUserId = "test-identity-id",
        DisplayName = "Test User",
        Email = "test@example.com",
        Projects = new List<Project>()
    };

    public ReportServiceTests()
    {
        _service = new ReportService(
            _taskRepoMock.Object,
            _sessionRepoMock.Object,
            _profileRepoMock.Object,
            _calculator);

        _profileRepoMock
            .Setup(r => r.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
    }

    [Fact]
    public async Task GenerateProductivityReport_GroupsTasksByStatus()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            UserProfileId = _testUser.Id,
            Name = "Test Project",
            Status = ProjectStatus.Active
        };

        var tasks = new List<TaskItem>
        {
            CreateTask(project, TaskItemStatus.Completed, "Task A"),
            CreateTask(project, TaskItemStatus.Completed, "Task B"),
            CreateTask(project, TaskItemStatus.InProgress, "Task C"),
            CreateTask(project, TaskItemStatus.Todo, "Task D"),
        };

        _taskRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var report = await _service.GenerateProductivityReportAsync(
            _testUser.Id,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        // Assert
        report.UserDisplayName.Should().Be("Test User");
        report.TotalTasksCount.Should().Be(4);
        report.CompletedTasksCount.Should().Be(2);

        report.Groups.Should().HaveCount(3);
        report.Groups.Should().Contain(g =>
            g.Status == TaskItemStatus.Completed && g.TaskCount == 2);
        report.Groups.Should().Contain(g =>
            g.Status == TaskItemStatus.InProgress && g.TaskCount == 1);
        report.Groups.Should().Contain(g =>
            g.Status == TaskItemStatus.Todo && g.TaskCount == 1);
    }

    [Fact]
    public async Task GenerateProductivityReport_EmptyTaskList_ReturnsEmptyGroups()
    {
        _taskRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        var report = await _service.GenerateProductivityReportAsync(
            _testUser.Id,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        report.Groups.Should().BeEmpty();
        report.TotalTasksCount.Should().Be(0);
        report.TotalHoursLogged.Should().Be(0);
    }

    [Fact]
    public async Task GenerateProductivityReport_AllSameStatus_ReturnsSingleGroup()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            UserProfileId = _testUser.Id,
            Name = "Test Project",
            Status = ProjectStatus.Active
        };

        var tasks = new List<TaskItem>
        {
            CreateTask(project, TaskItemStatus.InProgress, "Task A"),
            CreateTask(project, TaskItemStatus.InProgress, "Task B"),
            CreateTask(project, TaskItemStatus.InProgress, "Task C"),
        };

        _taskRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var report = await _service.GenerateProductivityReportAsync(
            _testUser.Id,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        report.Groups.Should().HaveCount(1);
        report.Groups[0].Status.Should().Be(TaskItemStatus.InProgress);
        report.Groups[0].TaskCount.Should().Be(3);
    }

    [Fact]
    public async Task GenerateProductivityReport_AggregatesHoursPerGroup()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            UserProfileId = _testUser.Id,
            Name = "Test Project",
            Status = ProjectStatus.Active
        };

        var task = CreateTask(project, TaskItemStatus.Completed, "Task With Sessions");

        // Add a 2-hour completed session
        task.WorkSessions.Add(new WorkSession
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow.AddHours(-1),
            Status = SessionStatus.Completed
        });

        _taskRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { task });

        var report = await _service.GenerateProductivityReportAsync(
            _testUser.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        report.TotalHoursLogged.Should().BeApproximately(2.0, 0.1);
        report.Groups.Should().HaveCount(1);
        report.Groups[0].TotalHours.Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public async Task GenerateProductivityReport_InvalidUser_ThrowsException()
    {
        var invalidId = Guid.NewGuid();

        _profileRepoMock
            .Setup(r => r.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var act = () => _service.GenerateProductivityReportAsync(
            invalidId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{invalidId}*");
    }

    // ─── Helpers ────────────────────────────────────────────

    private static TaskItem CreateTask(Project project, TaskItemStatus status, string title)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Title = title,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            WorkSessions = new List<WorkSession>()
        };
    }
}
