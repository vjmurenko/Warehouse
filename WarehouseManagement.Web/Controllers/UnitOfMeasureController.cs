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
    /// <returns>List of all units of measure</returns>
    [HttpGet]
    public async Task<ActionResult<List<UnitOfMeasureDto>>> GetUnitsOfMeasure()
    {
        var units = await _unitOfMeasureService.GetAllAsync();
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
    /// <returns>List of active units of measure</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<UnitOfMeasureDto>>> GetActiveUnitsOfMeasure()
    {
        var units = await _unitOfMeasureService.GetActiveAsync();
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
    /// <returns>Unit of measure information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UnitOfMeasureDto>> GetUnitOfMeasureById(Guid id)
    {
        var unit = await _unitOfMeasureService.GetByIdAsync(id);
        
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
    /// <returns>ID of the created unit of measure</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUnitOfMeasure([FromBody] CreateUnitOfMeasureRequest request)
    {
        var unitId = await _unitOfMeasureService.CreateUnitOfMeasureAsync(request.Name);
        return CreatedAtAction(nameof(GetUnitOfMeasureById), new { id = unitId }, unitId);
    }

    /// <summary>
    /// Update an existing unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <param name="request">Unit of measure update data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUnitOfMeasure(Guid id, [FromBody] UpdateUnitOfMeasureRequest request)
    {
     
        var success = await _unitOfMeasureService.UpdateUnitOfMeasureAsync(id, request.Name);
        
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
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUnitOfMeasure(Guid id)
    {
        var success = await _unitOfMeasureService.DeleteAsync(id);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Archive a unit of measure
    /// </summary>
    /// <param name="id">The unit of measure ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult> ArchiveUnitOfMeasure(Guid id)
    {
        var success = await _unitOfMeasureService.ArchiveAsync(id);
        
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
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateUnitOfMeasure(Guid id)
    {
        var success = await _unitOfMeasureService.ActivateAsync(id);
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}