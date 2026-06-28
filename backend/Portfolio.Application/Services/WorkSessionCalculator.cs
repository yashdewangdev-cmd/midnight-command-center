using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Application.Services;

/// <summary>
/// Computes cumulative work session durations across multiple disconnected sessions.
/// Handles active, paused, and completed sessions with correct midnight-spanning logic.
/// </summary>
public class WorkSessionCalculator : IWorkSessionCalculator
{
    /// <inheritdoc />
    public TimeSpan CalculateTotalTime(IEnumerable<WorkSession> sessions)
    {
        return sessions.Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);
    }

    /// <inheritdoc />
    public TimeSpan CalculateDailyTime(IEnumerable<WorkSession> sessions, DateTime date)
    {
        var dayStart = date.Date; // midnight UTC of the target date
        var dayEnd = dayStart.AddDays(1);

        var totalDuration = TimeSpan.Zero;

        foreach (var session in sessions)
        {
            var effectiveEnd = session.Status == SessionStatus.Active
                ? DateTime.UtcNow
                : session.EndTime ?? session.StartTime;

            // Skip sessions entirely outside the target day
            if (effectiveEnd <= dayStart || session.StartTime >= dayEnd)
                continue;

            // Clamp session boundaries to the target day
            var clampedStart = session.StartTime < dayStart ? dayStart : session.StartTime;
            var clampedEnd = effectiveEnd > dayEnd ? dayEnd : effectiveEnd;

            var duration = clampedEnd - clampedStart;
            if (duration > TimeSpan.Zero)
            {
                totalDuration += duration;
            }
        }

        return totalDuration;
    }

    /// <inheritdoc />
    public Dictionary<Guid, TimeSpan> CalculateTimeByTask(IEnumerable<WorkSession> sessions)
    {
        return sessions
            .GroupBy(s => s.TaskItemId)
            .ToDictionary(
                group => group.Key,
                group => CalculateTotalTime(group));
    }
}
