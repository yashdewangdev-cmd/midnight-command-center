using Portfolio.Domain.Entities;

namespace Portfolio.Application.Interfaces;

/// <summary>
/// Repository contract for Project entity CRUD operations.
/// All methods are async for EF Core PostgreSQL provider compatibility.
/// </summary>
public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Project>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken ct = default);
    Task<Project> CreateAsync(Project project, CancellationToken ct = default);
    Task<Project> UpdateAsync(Project project, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
