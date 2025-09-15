using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Dtos.UntOfMeasure;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitOfMeasureController : ControllerBase
{
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public UnitOfMeasureController(IUnitOfMeasureService unitOfMeasureService)
    {
        _unitOfMeasureService = unitOfMeasureService;
    }

    /// <summary>
    /// Get all units of measure
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of all units of measure</returns>
    [HttpGet]
    public async Task<ActionResult<List<UnitOfMeasureDto>>> GetUnitsOfMeasure(CancellationToken ctx)
    {
        var units = await _unitOfMeasureService.GetAllAsync(ctx);
        var unitDtos = units.Select(u => new UnitOfMeasureDto(
            u.Id,
            u.Name,
            u.IsActive
        )).ToList();
        
        return Ok(unitDtos);
    }

    /// <summary>
    /// Get active units of measure only
    /// </summary>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>List of active units of measure</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<UnitOfMeasureDto>>> GetActiveUnitsOfMeasure(CancellationToken ctx)
    {
        var units = await _unitOfMeasureService.GetActiveAsync(ctx);
        var unitDtos = units.Select(u => new UnitOfMeasureDto(
            u.Id,
            u.Name,
            u.IsActive
        )).ToList();
        
        return Ok(unitDtos);
    }

    /// <summary>
    /// Get a specific unit of measure by ID
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>Unit of measure information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UnitOfMeasureDto>> GetUnitOfMeasureById(Guid id, CancellationToken ctx)
    {
        var unit = await _unitOfMeasureService.GetByIdAsync(id, ctx);
        
        if (unit == null)
        {
            return NotFound();
        }
        
        var unitDto = new UnitOfMeasureDto(
            unit.Id,
            unit.Name,
            unit.IsActive
        );
        
        return Ok(unitDto);
    }

    /// <summary>
    /// Create a new unit of measure
    /// </summary>
    /// <param name="request">Unit of measure creation data</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>ID of the created unit of measure</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUnitOfMeasure([FromBody] CreateUnitOfMeasureRequest request, CancellationToken ctx)
    {
        var unitId = await _unitOfMeasureService.CreateUnitOfMeasureAsync(request.Name, ctx);
        return CreatedAtAction(nameof(GetUnitOfMeasureById), new { id = unitId }, unitId);
    }

    /// <summary>
    /// Update an existing unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="request">Unit of measure update data</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUnitOfMeasure(Guid id, [FromBody] UpdateUnitOfMeasureRequest request, CancellationToken ctx)
    {
        var success = await _unitOfMeasureService.UpdateUnitOfMeasureAsync(id, request.Name, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Delete a unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUnitOfMeasure(Guid id, CancellationToken ctx)
    {
        await _unitOfMeasureService.DeleteAsync(id, ctx);
        return NoContent();
    }

    /// <summary>
    /// Archive a unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveUnitOfMeasure(Guid id, CancellationToken ctx)
    {
        var success = await _unitOfMeasureService.ArchiveAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Activate a unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="ctx">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateUnitOfMeasure(Guid id, CancellationToken ctx)
    {
        var success = await _unitOfMeasureService.ActivateAsync(id, ctx);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}