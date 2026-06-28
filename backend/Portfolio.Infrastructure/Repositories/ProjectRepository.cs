using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly PortfolioDbContext _context;

    public ProjectRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.UserProfile)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
                .ThenInclude(t => t.WorkSessions)
            .Include(p => p.UserProfile)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IEnumerable<Project>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .Where(p => p.UserProfileId == userProfileId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken ct = default)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync([id], ct);
        if (project is null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
