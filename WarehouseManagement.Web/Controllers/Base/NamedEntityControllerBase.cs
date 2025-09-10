using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Web.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class NamedEntityControllerBase<TDto, TGetAllQuery, TGetActiveQuery, TGetByIdQuery, TCreateCommand, TUpdateCommand, TDeleteCommand, TArchiveCommand, TActivateCommand> 
    : ControllerBase
    where TDto : class, INamedEntityDto
    where TGetAllQuery : IRequest<List<TDto>>, new()
    where TGetActiveQuery : IRequest<List<TDto>>, new()
    where TGetByIdQuery : IRequest<TDto?>
    where TCreateCommand : IRequest<Guid>
    where TUpdateCommand : IRequest<Unit>
    where TDeleteCommand : IRequest<Unit>
    where TArchiveCommand : IRequest<Unit>
    where TActivateCommand : IRequest<Unit>
{
    protected readonly IMediator _mediator;

    protected NamedEntityControllerBase(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected abstract TGetByIdQuery CreateGetByIdQuery(Guid id);
    protected abstract TDeleteCommand CreateDeleteCommand(Guid id);
    protected abstract TArchiveCommand CreateArchiveCommand(Guid id);
    protected abstract TActivateCommand CreateActivateCommand(Guid id);
    protected abstract string GetEntityName();

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>List of all entities</returns>
    [HttpGet]
    public virtual async Task<ActionResult<List<TDto>>> GetAll()
    {
        var query = new TGetAllQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get active entities only
    /// </summary>
    /// <returns>List of active entities</returns>
    [HttpGet("active")]
    public virtual async Task<ActionResult<List<TDto>>> GetActive()
    {
        var query = new TGetActiveQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>Entity information</returns>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id)
    {
        var query = CreateGetByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="command">Entity creation command</param>
    /// <returns>ID of the created entity</returns>
    [HttpPost]
    public virtual async Task<ActionResult<Guid>> Create([FromBody] TCreateCommand command)
    {
        var entityId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = entityId }, entityId);
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="command">Entity update command</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult> Update(Guid id, [FromBody] TUpdateCommand command)
    {
        // Note: ID validation should be done in the command handler
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete(Guid id)
    {
        var command = CreateDeleteCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Archive an entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/archive")]
    public virtual async Task<ActionResult> Archive(Guid id)
    {
        var command = CreateArchiveCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Activate an entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/activate")]
    public virtual async Task<ActionResult> Activate(Guid id)
    {
        var command = CreateActivateCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}