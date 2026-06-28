using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Api.Controllers;

/// <summary>
/// OData-enabled controller for TaskItem entities.
/// Routes: /odata/Tasks, /odata/Tasks({key})
/// Includes custom actions for work session management.
/// </summary>
public class TasksController : ODataController
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkSessionRepository _sessionRepository;

    public TasksController(
        ITaskRepository taskRepository,
        IWorkSessionRepository sessionRepository)
    {
        _taskRepository = taskRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// GET /odata/Tasks — returns all tasks (authenticated).
    /// </summary>
    [HttpGet]
    [Authorize]
    [EnableQuery(MaxTop = 100)]
    public async Task<ActionResult<IEnumerable<TaskItem>>> Get()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return Ok(tasks.AsQueryable());
    }

    /// <summary>
    /// GET /odata/Tasks({key}) — returns a single task.
    /// </summary>
    [HttpGet]
    [Authorize]
    [EnableQuery]
    public async Task<ActionResult<TaskItem>> Get(Guid key)
    {
        var task = await _taskRepository.GetByIdAsync(key);
        if (task is null)
            return NotFound();

        return Ok(task);
    }

    /// <summary>
    /// POST /odata/Tasks — creates a new task (authenticated).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TaskItem>> Post([FromBody] TaskItem taskItem)
    {
        taskItem.Id = Guid.NewGuid();
        taskItem.CreatedAt = DateTime.UtcNow;
        taskItem.UpdatedAt = DateTime.UtcNow;

        var created = await _taskRepository.CreateAsync(taskItem);
        return Created(created);
    }

    /// <summary>
    /// PUT /odata/Tasks({key}) — updates a task (authenticated).
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<TaskItem>> Put(Guid key, [FromBody] TaskItem taskItem)
    {
        var existing = await _taskRepository.GetByIdAsync(key);
        if (existing is null)
            return NotFound();

        existing.Title = taskItem.Title;
        existing.Description = taskItem.Description;
        existing.Priority = taskItem.Priority;
        existing.Status = taskItem.Status;
        existing.DueDate = taskItem.DueDate;

        if (taskItem.Status == TaskItemStatus.Completed && existing.CompletedAt is null)
            existing.CompletedAt = DateTime.UtcNow;

        var updated = await _taskRepository.UpdateAsync(existing);
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /odata/Tasks({key}) — deletes a task (authenticated).
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> Delete(Guid key)
    {
        var deleted = await _taskRepository.DeleteAsync(key);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // ─── Work Session Actions ──────────────────────────────────────────

    /// <summary>
    /// POST /odata/Tasks({key})/StartSession — starts a new work session on the task.
    /// Returns 409 Conflict if a session is already active.
    /// </summary>
    [HttpPost("odata/Tasks({key})/StartSession")]
    [Authorize]
    public async Task<ActionResult<WorkSession>> StartSession(Guid key)
    {
        var task = await _taskRepository.GetByIdAsync(key);
        if (task is null)
            return NotFound(new { message = "Task not found." });

        // Check for existing active session
        var activeSession = await _sessionRepository.GetActiveSessionForTaskAsync(key);
        if (activeSession is not null)
            return Conflict(new { message = "An active session already exists for this task.", sessionId = activeSession.Id });

        // Auto-transition task to InProgress
        if (task.Status == TaskItemStatus.Todo)
        {
            task.Status = TaskItemStatus.InProgress;
            await _taskRepository.UpdateAsync(task);
        }

        var session = new WorkSession
        {
            Id = Guid.NewGuid(),
            TaskItemId = key,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        var created = await _sessionRepository.CreateAsync(session);
        return Ok(created);
    }

    /// <summary>
    /// POST /odata/Tasks({key})/StopSession — completes the active work session on the task.
    /// </summary>
    [HttpPost("odata/Tasks({key})/StopSession")]
    [Authorize]
    public async Task<ActionResult<WorkSession>> StopSession(Guid key, [FromBody] StopSessionRequest? request = null)
    {
        var activeSession = await _sessionRepository.GetActiveSessionForTaskAsync(key);
        if (activeSession is null)
            return NotFound(new { message = "No active session found for this task." });

        activeSession.EndTime = DateTime.UtcNow;
        activeSession.Status = SessionStatus.Completed;
        activeSession.Notes = request?.Notes;

        var updated = await _sessionRepository.UpdateAsync(activeSession);
        return Ok(updated);
    }

    /// <summary>
    /// POST /odata/Tasks({key})/PauseSession — pauses the active work session.
    /// </summary>
    [HttpPost("odata/Tasks({key})/PauseSession")]
    [Authorize]
    public async Task<ActionResult<WorkSession>> PauseSession(Guid key)
    {
        var activeSession = await _sessionRepository.GetActiveSessionForTaskAsync(key);
        if (activeSession is null)
            return NotFound(new { message = "No active session found for this task." });

        activeSession.EndTime = DateTime.UtcNow;
        activeSession.Status = SessionStatus.Paused;

        var updated = await _sessionRepository.UpdateAsync(activeSession);
        return Ok(updated);
    }
}

public class StopSessionRequest
{
    public string? Notes { get; set; }
}
