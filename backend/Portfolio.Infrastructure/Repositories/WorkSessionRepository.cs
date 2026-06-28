using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Data;

namespace Portfolio.Infrastructure.Repositories;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly PortfolioDbContext _context;

    public WorkSessionRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkSession>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _context.WorkSessions
            .Include(ws => ws.TaskItem)
            .Where(ws => ws.TaskItemId == taskId)
            .OrderByDescending(ws => ws.StartTime)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<WorkSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.WorkSessions
            .Include(ws => ws.TaskItem)
            .FirstOrDefaultAsync(ws => ws.Id == id, ct);
    }

    public async Task<WorkSession?> GetActiveSessionForTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _context.WorkSessions
            .Include(ws => ws.TaskItem)
            .FirstOrDefaultAsync(ws =>
                ws.TaskItemId == taskId && ws.Status == SessionStatus.Active, ct);
    }

    public async Task<IEnumerable<WorkSession>> GetSessionsByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _context.WorkSessions
            .Include(ws => ws.TaskItem)
                .ThenInclude(t => t.Project)
            .Where(ws =>
                ws.StartTime < dayEnd &&
                (ws.EndTime == null || ws.EndTime > dayStart))
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<WorkSession> CreateAsync(WorkSession session, CancellationToken ct = default)
    {
        _context.WorkSessions.Add(session);
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<WorkSession> UpdateAsync(WorkSession session, CancellationToken ct = default)
    {
        _context.WorkSessions.Update(session);
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _context.WorkSessions.FindAsync([id], ct);
        if (session is null) return false;

        _context.WorkSessions.Remove(session);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
