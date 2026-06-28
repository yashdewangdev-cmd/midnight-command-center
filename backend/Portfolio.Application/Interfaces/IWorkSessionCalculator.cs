using Portfolio.Domain.Entities;

namespace Portfolio.Application.Interfaces;

/// <summary>
/// Service for computing cumulative work session durations.
/// Handles multi-session scenarios with mixed statuses.
/// </summary>
public interface IWorkSessionCalculator
{
    /// <summary>
    /// Calculates total time spent on a task across all its sessions.
    /// </summary>
    TimeSpan CalculateTotalTime(IEnumerable<WorkSession> sessions);

    /// <summary>
    /// Calculates total time spent on a specific UTC date,
    /// handling sessions that span midnight correctly.
    /// </summary>
    TimeSpan CalculateDailyTime(IEnumerable<WorkSession> sessions, DateTime date);

    /// <summary>
    /// Calculates total time grouped by task for a collection of sessions.
    /// </summary>
    Dictionary<Guid, TimeSpan> CalculateTimeByTask(IEnumerable<WorkSession> sessions);
}
