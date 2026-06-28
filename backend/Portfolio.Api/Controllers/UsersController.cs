using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Portfolio.Application.Interfaces;
using Portfolio.Domain.Entities;

namespace Portfolio.Api.Controllers;

/// <summary>
/// OData-enabled controller for UserProfile entities.
/// Routes: /odata/Users, /odata/Users({key})
/// </summary>
public class UsersController : ODataController
{
    private readonly IUserProfileRepository _repository;

    public UsersController(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// GET /odata/Users — returns all user profiles (public).
    /// Supports $select, $expand, $filter, $orderby, $top.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [EnableQuery(MaxTop = 50)]
    public async Task<ActionResult<IEnumerable<UserProfile>>> Get()
    {
        var profiles = await _repository.GetAllAsync();
        return Ok(profiles.AsQueryable());
    }

    /// <summary>
    /// GET /odata/Users({key}) — returns a single user profile (public).
    /// Compliant with OData URL key format.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [EnableQuery]
    public async Task<ActionResult<UserProfile>> Get(Guid key)
    {
        var profile = await _repository.GetByIdAsync(key);
        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// PUT /odata/Users({key}) — updates a user profile (authenticated).
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<UserProfile>> Put(Guid key, [FromBody] UserProfile profile)
    {
        var existing = await _repository.GetByIdAsync(key);
        if (existing is null)
            return NotFound();

        existing.DisplayName = profile.DisplayName;
        existing.Bio = profile.Bio;
        existing.AvatarUrl = profile.AvatarUrl;
        existing.GitHubUrl = profile.GitHubUrl;
        existing.LinkedInUrl = profile.LinkedInUrl;

        var updated = await _repository.UpdateAsync(existing);
        return Ok(updated);
    }
}
