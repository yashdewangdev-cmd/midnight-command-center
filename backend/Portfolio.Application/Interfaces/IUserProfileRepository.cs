using Portfolio.Domain.Entities;

namespace Portfolio.Application.Interfaces;

/// <summary>
/// Repository contract for UserProfile entity.
/// </summary>
public interface IUserProfileRepository
{
    Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken ct = default);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default);
    Task<UserProfile> CreateAsync(UserProfile profile, CancellationToken ct = default);
    Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken ct = default);
}
