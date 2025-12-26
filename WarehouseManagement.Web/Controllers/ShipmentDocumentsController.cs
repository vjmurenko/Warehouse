using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipmentDocumentsController(IMediator mediator, ILogger<ShipmentDocumentsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all shipment documents with optional filtering
    /// </summary>
    /// <param name="fromDate">Filter documents from this date</param>
    /// <param name="toDate">Filter documents to this date</param>
    /// <param name="documentNumbers">Filter by specific document numbers</param>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <param name="clientIds">Filter by specific client IDs</param>
    /// <returns>List of shipment document summaries</returns>
    [HttpGet]
    public async Task<ActionResult<List<ShipmentDocumentDto>>> GetShipments(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] List<string>? documentNumbers = null,
        [FromQuery] List<Guid>? resourceIds = null,
        [FromQuery] List<Guid>? unitIds = null,
        [FromQuery] List<Guid>? clientIds = null)
    {
        logger.LogInformation("Getting shipment documents with filters - FromDate: {FromDate}, ToDate: {ToDate}, DocumentNumbers: {DocumentNumbersCount}, ResourceIds: {ResourceIdsCount}, UnitIds: {UnitIdsCount}, ClientIds: {ClientIdsCount}",
            fromDate, toDate, documentNumbers?.Count ?? 0, resourceIds?.Count ?? 0, unitIds?.Count ?? 0, clientIds?.Count ?? 0);
        
        var query = new GetShipmentsQuery(fromDate, toDate, documentNumbers, resourceIds, unitIds, clientIds);
        var result = await mediator.Send(query);
        
        logger.LogInformation("Successfully retrieved {Count} shipment documents", result.Count);
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
        logger.LogInformation("Getting shipment document by ID: {ShipmentId}", id);
        var result = await mediator.Send(new GetShipmentByIdQuery(id));
        
        if (result is null)
        {
            logger.LogWarning("Shipment document with ID {ShipmentId} not found", id);
            return NotFound();
        }
        
        logger.LogInformation("Successfully retrieved shipment document with ID: {ShipmentId}", id);
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
        logger.LogInformation("Creating new shipment document");
        var result = await mediator.Send(request);
        logger.LogInformation("Successfully created shipment document with ID: {ShipmentId}", result);
        return CreatedAtAction(nameof(GetShipmentById), new { id = result }, result);
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

    /// <summary>
    /// Delete a shipment document
    /// </summary>
    /// <param name="id">The shipment document ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShipment(Guid id)
    {
        logger.LogInformation("Deleting shipment document with ID: {ShipmentId}", id);
        await mediator.Send(new DeleteShipmentCommand(id));
        logger.LogInformation("Successfully deleted shipment document with ID: {ShipmentId}", id);
        return NoContent();
    }

    /// <summary>
    /// Revoke a signed shipment document
    /// </summary>
    /// <param name="id">The shipment document ID</param>
    /// <returns>No content if successful</returns>
    [HttpPost("{id}/revoke")]
    public async Task<ActionResult> RevokeShipment(Guid id)
    {
        logger.LogInformation("Revoking shipment document with ID: {ShipmentId}", id);
        await mediator.Send(new RevokeShipmentCommand(id));
        logger.LogInformation("Successfully revoked shipment document with ID: {ShipmentId}", id);
        return NoContent();
    }
}