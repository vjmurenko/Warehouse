using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Dtos.Resource;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    /// <summary>
    /// Get all resources
    /// </summary>
    /// <returns>List of all resources</returns>
    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetResources()
    {
        var resources = await _resourceService.GetAllAsync();
        var resourceDtos = resources.Select(r => new ResourceDto(
            r.Id,
            r.Name,
            r.IsActive
        )).ToList();
        
        return Ok(resourceDtos);
    }

    /// <summary>
    /// Get active resources only
    /// </summary>
    /// <returns>List of active resources</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ResourceDto>>> GetActiveResources()
    {
        var resources = await _resourceService.GetActiveAsync();
        var resourceDtos = resources.Select(r => new ResourceDto(
            r.Id,
            r.Name,
            r.IsActive
        )).ToList();
        
        return Ok(resourceDtos);
    }

    /// <summary>
    /// Get a specific resource by ID
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <returns>Resource information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetResourceById(Guid id)
    {
        var resource = await _resourceService.GetByIdAsync(id);
        
        if (resource == null)
        {
            return NotFound();
        }
        
        var resourceDto = new ResourceDto(
            resource.Id,
            resource.Name,
            resource.IsActive
        );
        
        return Ok(resourceDto);
    }

    /// <summary>
    /// Create a new resource
    /// </summary>
    /// <param name="request">Resource creation data</param>
    /// <returns>ID of the created resource</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateResource([FromBody] CreateResourceRequest request)
    {
        var resourceId = await _resourceService.CreateResourceAsync(request.Name);
        return CreatedAtAction(nameof(GetResourceById), new { id = resourceId }, resourceId);
    }

    /// <summary>
    /// Update an existing resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <param name="request">Resource update data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceRequest request)
    {
        var success = await _resourceService.UpdateResourceAsync(id, request.Name);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Delete a resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteResource(Guid id)
    {
        await _resourceService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Archive a resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveResource(Guid id)
    {
        var success = await _resourceService.ArchiveAsync(id);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Activate a resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateResource(Guid id)
    {
        var success = await _resourceService.ActivateAsync(id);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
