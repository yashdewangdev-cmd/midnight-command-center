using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Api.Controllers;

/// <summary>
/// OData-enabled controller for Project entities.
/// Routes: /odata/Projects, /odata/Projects({key})
/// </summary>
public class ProjectsController : ODataController
{
    private readonly IProjectRepository _repository;

    public ProjectsController(IProjectRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// GET /odata/Projects — returns all projects (public for portfolio).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [EnableQuery(MaxTop = 100)]
    public async Task<ActionResult<IEnumerable<Project>>> Get()
    {
        var projects = await _repository.GetAllAsync();
        return Ok(projects.AsQueryable());
    }

    /// <summary>
    /// GET /odata/Projects({key}) — returns a single project.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [EnableQuery]
    public async Task<ActionResult<Project>> Get(Guid key)
    {
        var project = await _repository.GetByIdAsync(key);
        if (project is null)
            return NotFound();

        return Ok(project);
    }

    /// <summary>
    /// POST /odata/Projects — creates a new project (authenticated).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Project>> Post([FromBody] Project project)
    {
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(project);
        return Created(created);
    }

    /// <summary>
    /// PUT /odata/Projects({key}) — updates a project (authenticated).
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<Project>> Put(Guid key, [FromBody] Project project)
    {
        var existing = await _repository.GetByIdAsync(key);
        if (existing is null)
            return NotFound();

        existing.Name = project.Name;
        existing.Description = project.Description;
        existing.Status = project.Status;
        existing.TechnologyStack = project.TechnologyStack;
        existing.RepositoryUrl = project.RepositoryUrl;
        existing.LiveUrl = project.LiveUrl;
        existing.ImageUrl = project.ImageUrl;

        if (project.Status == ProjectStatus.Completed && existing.CompletedAt is null)
            existing.CompletedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /odata/Projects({key}) — deletes a project (authenticated).
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> Delete(Guid key)
    {
        var deleted = await _repository.DeleteAsync(key);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
