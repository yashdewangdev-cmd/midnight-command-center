using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly PortfolioDbContext _context;

    public TaskRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.TaskItems
            .Include(t => t.WorkSessions)
            .Include(t => t.Project)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TaskItems
            .Include(t => t.WorkSessions)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.TaskItems
            .Include(t => t.WorkSessions)
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskItemStatus status, CancellationToken ct = default)
    {
        return await _context.TaskItems
            .Include(t => t.WorkSessions)
            .Include(t => t.Project)
            .Where(t => t.Status == status)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.TaskItems.Update(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _context.TaskItems.FindAsync([id], ct);
        if (task is null) return false;

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
