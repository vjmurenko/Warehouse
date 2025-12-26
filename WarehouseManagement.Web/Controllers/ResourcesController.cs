using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Dtos.Resource;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all resources
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of all resources</returns>
    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetResources(CancellationToken ctx)
    {
        _logger.LogInformation("Getting all resources");
        var resources = await _resourceService.GetAllAsync(ctx);
        var resourceDtos = resources.Select(r => new ResourceDto(
            r.Id,
            r.Name,
            r.IsActive
        )).ToList();
        
        _logger.LogInformation("Successfully retrieved {Count} resources", resourceDtos.Count);
        return Ok(resourceDtos);
    }

    /// <summary>
    /// Get active resources only
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of active resources</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ResourceDto>>> GetActiveResources(CancellationToken ctx)
    {
        var resources = await _resourceService.GetActiveAsync(ctx);
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
    /// <param name="ctx">Cancellation token</param>
    /// <returns>Resource information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetResourceById(Guid id, CancellationToken ctx)
    {
        var resource = await _resourceService.GetByIdAsync(id, ctx);
        
        if (resource is null)
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
    /// <param name="ctx">Cancellation token</param>
    /// <returns>ID of the created resource</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateResource([FromBody] CreateResourceRequest request, CancellationToken ctx)
    {
        _logger.LogInformation("Creating new resource with name: {ResourceName}", request.Name);
        var resourceId = await _resourceService.CreateResourceAsync(request.Name, ctx);
        _logger.LogInformation("Successfully created resource with ID: {ResourceId}", resourceId);
        return CreatedAtAction(nameof(GetResourceById), new { id = resourceId }, resourceId);
    }

    /// <summary>
    /// Update an existing resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <param name="request">Resource update data</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceRequest request, CancellationToken ctx)
    {
        var success = await _resourceService.UpdateResourceAsync(id, request.Name, ctx);
        
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
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteResource(Guid id, CancellationToken ctx)
    {
        _logger.LogInformation("Deleting resource with ID: {ResourceId}", id);
        await _resourceService.DeleteAsync(id, ctx);
        _logger.LogInformation("Successfully deleted resource with ID: {ResourceId}", id);
        return NoContent();
    }

    /// <summary>
    /// Archive a resource
    /// </summary>
    /// <param name="id">The resource ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveResource(Guid id, CancellationToken ctx)
    {
        var success = await _resourceService.ArchiveAsync(id, ctx);
        
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
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateResource(Guid id, CancellationToken ctx)
    {
        var success = await _resourceService.ActivateAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
