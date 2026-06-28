using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly PortfolioDbContext _context;

    public UserProfileRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.UserProfiles
            .Include(u => u.Projects)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.UserProfiles
            .Include(u => u.Projects)
                .ThenInclude(p => p.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default)
    {
        return await _context.UserProfiles
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId, ct);
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile, CancellationToken ct = default)
    {
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync(ct);
        return profile;
    }
}
