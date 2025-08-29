using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentDocumentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all shipment documents with optional filtering
    /// </summary>
    /// <param name="fromDate">Filter documents from this date</param>
    /// <param name="toDate">Filter documents to this date</param>
    /// <param name="documentNumbers">Filter by specific document numbers</param>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <returns>List of shipment document summaries</returns>
    [HttpGet]
    public async Task<ActionResult<List<ShipmentDocumentDto>>> GetShipments(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] List<string>? documentNumbers = null,
        [FromQuery] List<Guid>? resourceIds = null,
        [FromQuery] List<Guid>? unitIds = null)
    {
        var query = new GetShipmentsQuery(fromDate, toDate, documentNumbers, resourceIds, unitIds);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific shipment document by ID
    /// </summary>
    /// <param name="id">The shipment document ID</param>
    /// <returns>Shipment document with detailed information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShipmentDocumentDto>> GetShipmentById(Guid id)
    {
        var result = await mediator.Send(new GetShipmentByIdQuery(id));
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new shipment document
    /// </summary>
    /// <param name="request">Shipment document creation data</param>
    /// <returns>ID of the created shipment document</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateShipment([FromBody] CreateShipmentCommand request)
    {
        try
        {
            var result = await mediator.Send(request);
            return CreatedAtAction(nameof(GetShipmentById), new { id = result }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing shipment document
    /// </summary>
    /// <param name="id">The shipment document ID</param>
    /// <param name="request">Shipment document update data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateShipment([FromBody] UpdateShipmentCommand request)
    {
        await mediator.Send(request);
        return NoContent();
    }

    // Signing is handled during creation or update, no separate endpoint needed

    /// <summary>
    /// Delete a shipment document
    /// </summary>
    /// <param name="id">The shipment document ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShipment(Guid id)
    {
        await mediator.Send(new DeleteShipmentCommand(id));
        return NoContent();
    }
}