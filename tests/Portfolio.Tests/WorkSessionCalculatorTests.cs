using FluentAssertions;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Tests;

/// <summary>
/// Unit tests for the WorkSessionCalculator service.
/// Validates multi-session cumulative time calculation across various scenarios.
/// </summary>
public class WorkSessionCalculatorTests
{
    private readonly WorkSessionCalculator _calculator = new();

    // ─── CalculateTotalTime ─────────────────────────────────

    [Fact]
    public void CalculateTotalTime_SingleCompletedSession_ReturnsCorrectDuration()
    {
        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1))
        };

        var result = _calculator.CalculateTotalTime(sessions);

        result.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateTotalTime_MultipleCompletedSessions_ReturnsCumulativeTotal()
    {
        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-5),
                DateTime.UtcNow.AddHours(-4)),
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-3),
                DateTime.UtcNow.AddHours(-1))
        };

        var result = _calculator.CalculateTotalTime(sessions);

        result.Should().BeCloseTo(TimeSpan.FromHours(3), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateTotalTime_ActiveSession_UsesUtcNowAsEnd()
    {
        var start = DateTime.UtcNow.AddMinutes(-30);
        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Active, start, null)
        };

        var result = _calculator.CalculateTotalTime(sessions);

        result.Should().BeCloseTo(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CalculateTotalTime_MixedStatuses_CalculatesCorrectly()
    {
        var sessions = new List<WorkSession>
        {
            // Completed: 1 hour
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-4),
                DateTime.UtcNow.AddHours(-3)),
            // Paused: 30 minutes (EndTime set at pause)
            CreateSession(SessionStatus.Paused,
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1.5)),
            // Active: ~15 minutes
            CreateSession(SessionStatus.Active,
                DateTime.UtcNow.AddMinutes(-15),
                null)
        };

        var result = _calculator.CalculateTotalTime(sessions);

        // Should be approximately 1h + 0.5h + 0.25h = 1.75h
        result.Should().BeCloseTo(TimeSpan.FromHours(1.75), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void CalculateTotalTime_NoSessions_ReturnsZero()
    {
        var result = _calculator.CalculateTotalTime(new List<WorkSession>());

        result.Should().Be(TimeSpan.Zero);
    }

    // ─── CalculateDailyTime ─────────────────────────────────

    [Fact]
    public void CalculateDailyTime_SessionWithinDay_ReturnsFullDuration()
    {
        var today = DateTime.UtcNow.Date;
        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Completed,
                today.AddHours(9),
                today.AddHours(11))
        };

        var result = _calculator.CalculateDailyTime(sessions, today);

        result.Should().BeCloseTo(TimeSpan.FromHours(2), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateDailyTime_SessionSpanningMidnight_ClampsToTargetDay()
    {
        var today = DateTime.UtcNow.Date;
        var sessions = new List<WorkSession>
        {
            // Session starts at 10 PM yesterday, ends at 2 AM today
            CreateSession(SessionStatus.Completed,
                today.AddHours(-2),  // 10 PM yesterday
                today.AddHours(2))   // 2 AM today
        };

        // For today, should only count midnight (00:00) to 2 AM = 2 hours
        var result = _calculator.CalculateDailyTime(sessions, today);
        result.Should().BeCloseTo(TimeSpan.FromHours(2), TimeSpan.FromSeconds(1));

        // For yesterday, should only count 10 PM to midnight = 2 hours
        var yesterday = today.AddDays(-1);
        var resultYesterday = _calculator.CalculateDailyTime(sessions, yesterday);
        resultYesterday.Should().BeCloseTo(TimeSpan.FromHours(2), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalculateDailyTime_SessionOutsideTargetDay_ReturnsZero()
    {
        var today = DateTime.UtcNow.Date;
        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Completed,
                today.AddDays(-3).AddHours(9),
                today.AddDays(-3).AddHours(12))
        };

        var result = _calculator.CalculateDailyTime(sessions, today);

        result.Should().Be(TimeSpan.Zero);
    }

    // ─── CalculateTimeByTask ────────────────────────────────

    [Fact]
    public void CalculateTimeByTask_MultipleTaskSessions_GroupsCorrectly()
    {
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();

        var sessions = new List<WorkSession>
        {
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-4),
                DateTime.UtcNow.AddHours(-3),
                taskId1),
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1),
                taskId1),
            CreateSession(SessionStatus.Completed,
                DateTime.UtcNow.AddHours(-5),
                DateTime.UtcNow.AddHours(-4.5),
                taskId2),
        };

        var result = _calculator.CalculateTimeByTask(sessions);

        result.Should().HaveCount(2);
        result[taskId1].Should().BeCloseTo(TimeSpan.FromHours(2), TimeSpan.FromSeconds(1));
        result[taskId2].Should().BeCloseTo(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(1));
    }

    // ─── Helpers ────────────────────────────────────────────

    private static WorkSession CreateSession(
        SessionStatus status,
        DateTime start,
        DateTime? end,
        Guid? taskId = null)
    {
        return new WorkSession
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId ?? Guid.NewGuid(),
            StartTime = start,
            EndTime = end,
            Status = status,
        };
    }
}
