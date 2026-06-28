using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Portfolio.Application.Services;

/// <summary>
/// Generates structured productivity reports grouped by task status,
/// and compiles them into downloadable PDF documents using QuestPDF.
/// </summary>
public class ReportService : IReportService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkSessionRepository _sessionRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IWorkSessionCalculator _calculator;

    public ReportService(
        ITaskRepository taskRepository,
        IWorkSessionRepository sessionRepository,
        IUserProfileRepository userProfileRepository,
        IWorkSessionCalculator calculator)
    {
        _taskRepository = taskRepository;
        _sessionRepository = sessionRepository;
        _userProfileRepository = userProfileRepository;
        _calculator = calculator;
    }

    /// <inheritdoc />
    public async Task<ProductivityReportDto> GenerateProductivityReportAsync(
        Guid userProfileId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(userProfileId, ct);
        if (userProfile is null)
            throw new InvalidOperationException($"User profile {userProfileId} not found.");

        var allTasks = await _taskRepository.GetAllAsync(ct);
        var userTasks = allTasks
            .Where(t => t.Project?.UserProfileId == userProfileId)
            .ToList();

        // Build groups by status
        var groups = new List<TaskReportGroupDto>();
        foreach (var status in Enum.GetValues<TaskItemStatus>())
        {
            var tasksInGroup = userTasks
                .Where(t => t.Status == status)
                .ToList();

            if (tasksInGroup.Count == 0)
                continue;

            var taskDtos = tasksInGroup.Select(t =>
            {
                var sessions = t.WorkSessions
                    .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
                    .ToList();

                return new TaskItemDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project?.Name ?? "Unknown",
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    DueDate = t.DueDate,
                    TotalHoursLogged = _calculator.CalculateTotalTime(sessions).TotalHours,
                    SessionCount = sessions.Count,
                    HasActiveSession = sessions.Any(s => s.Status == SessionStatus.Active)
                };
            }).ToList();

            var totalHours = taskDtos.Sum(t => t.TotalHoursLogged);

            groups.Add(new TaskReportGroupDto
            {
                Status = status,
                StatusLabel = FormatStatusLabel(status),
                TaskCount = taskDtos.Count,
                TotalHours = Math.Round(totalHours, 2),
                Tasks = taskDtos
            });
        }

        return new ProductivityReportDto
        {
            UserProfileId = userProfileId,
            UserDisplayName = userProfile.DisplayName,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            TotalHoursLogged = Math.Round(groups.Sum(g => g.TotalHours), 2),
            TotalTasksCount = groups.Sum(g => g.TaskCount),
            CompletedTasksCount = groups
                .Where(g => g.Status == TaskItemStatus.Completed)
                .Sum(g => g.TaskCount),
            Groups = groups
        };
    }

    /// <inheritdoc />
    public async Task<byte[]> GenerateProductivityPdfAsync(
        Guid userProfileId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        var report = await GenerateProductivityReportAsync(userProfileId, startDate, endDate, ct);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Text("Productivity Report")
                        .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(4).Text(
                        $"{report.UserDisplayName} • {report.StartDate:MMM dd, yyyy} — {report.EndDate:MMM dd, yyyy}")
                        .FontSize(11).FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Summary row
                    col.Item().PaddingBottom(15).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Total Hours").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{report.TotalHoursLogged:F1}h")
                                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Total Tasks").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{report.TotalTasksCount}")
                                .FontSize(20).Bold().FontColor(Colors.Teal.Darken2);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Completed").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{report.CompletedTasksCount}")
                                .FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                        });
                    });

                    // Groups
                    foreach (var group in report.Groups)
                    {
                        col.Item().PaddingBottom(10).Column(groupCol =>
                        {
                            groupCol.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text($"{group.StatusLabel} ({group.TaskCount})")
                                    .Bold().FontSize(12);
                                row.ConstantItem(100).AlignRight()
                                    .Text($"{group.TotalHours:F1}h")
                                    .FontSize(12).FontColor(Colors.Blue.Darken1);
                            });

                            // Task table
                            groupCol.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Title
                                    columns.RelativeColumn(2); // Project
                                    columns.RelativeColumn(1); // Priority
                                    columns.ConstantColumn(60); // Hours
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().PaddingBottom(4)
                                        .Text("Task").Bold().FontSize(9);
                                    header.Cell().PaddingBottom(4)
                                        .Text("Project").Bold().FontSize(9);
                                    header.Cell().PaddingBottom(4)
                                        .Text("Priority").Bold().FontSize(9);
                                    header.Cell().PaddingBottom(4).AlignRight()
                                        .Text("Hours").Bold().FontSize(9);
                                });

                                foreach (var task in group.Tasks)
                                {
                                    table.Cell().PaddingVertical(2)
                                        .Text(task.Title).FontSize(9);
                                    table.Cell().PaddingVertical(2)
                                        .Text(task.ProjectName).FontSize(9).FontColor(Colors.Grey.Darken1);
                                    table.Cell().PaddingVertical(2)
                                        .Text(task.Priority.ToString()).FontSize(9);
                                    table.Cell().PaddingVertical(2).AlignRight()
                                        .Text($"{task.TotalHoursLogged:F1}").FontSize(9);
                                }
                            });
                        });
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span($"{report.GeneratedAt:yyyy-MM-dd HH:mm UTC}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatStatusLabel(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Todo => "📋 To Do",
        TaskItemStatus.InProgress => "🔄 In Progress",
        TaskItemStatus.Completed => "✅ Completed",
        TaskItemStatus.Cancelled => "❌ Cancelled",
        _ => status.ToString()
    };
}
