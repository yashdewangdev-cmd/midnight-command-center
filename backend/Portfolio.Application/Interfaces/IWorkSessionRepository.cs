using Portfolio.Domain.Entities;

namespace Portfolio.Application.Interfaces;

/// <summary>
/// Repository contract for WorkSession entity CRUD and session management.
/// </summary>
public interface IWorkSessionRepository
{
    Task<IEnumerable<WorkSession>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<WorkSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkSession?> GetActiveSessionForTaskAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>
    /// Returns all sessions for a given date (UTC), across all tasks.
    /// </summary>
    Task<IEnumerable<WorkSession>> GetSessionsByDateAsync(DateTime date, CancellationToken ct = default);

    Task<WorkSession> CreateAsync(WorkSession session, CancellationToken ct = default);
    Task<WorkSession> UpdateAsync(WorkSession session, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
