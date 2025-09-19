using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptDocumentsController(IMediator mediator, ILogger<ReceiptDocumentsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all receipt documents with optional filtering
    /// </summary>
    /// <param name="fromDate">Filter documents from this date</param>
    /// <param name="toDate">Filter documents to this date</param>
    /// <param name="documentNumbers">Filter by specific document numbers</param>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <returns>List of receipt documents with full details</returns>
    [HttpGet]
    public async Task<ActionResult<List<ReceiptDocumentDto>>> GetReceipts(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] List<string>? documentNumbers = null,
        [FromQuery] List<Guid>? resourceIds = null,
        [FromQuery] List<Guid>? unitIds = null)
    {
        logger.LogInformation("Getting receipt documents with filters - FromDate: {FromDate}, ToDate: {ToDate}, DocumentNumbers: {DocumentNumbersCount}, ResourceIds: {ResourceIdsCount}, UnitIds: {UnitIdsCount}",
            fromDate, toDate, documentNumbers?.Count ?? 0, resourceIds?.Count ?? 0, unitIds?.Count ?? 0);
        
        var query = new GetReceiptsQuery(fromDate, toDate, documentNumbers, resourceIds, unitIds);
        var result = await mediator.Send(query);
        
        logger.LogInformation("Successfully retrieved {Count} receipt documents", result.Count);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific receipt document by ID
    /// </summary>
    /// <param name="id">The receipt document ID</param>
    /// <returns>Receipt document with detailed information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReceiptDocumentDto>> GetReceiptById(Guid id)
    {
        logger.LogInformation("Getting receipt document by ID: {ReceiptId}", id);
        var result = await mediator.Send(new GetReceiptByIdQuery(id));
        
        if (result == null)
        {
            logger.LogWarning("Receipt document with ID {ReceiptId} not found", id);
            return NotFound();
        }
        
        logger.LogInformation("Successfully retrieved receipt document with ID: {ReceiptId}", id);
        return Ok(result);
    }

    /// <summary>
    /// Create a new receipt document
    /// </summary>
    /// <param name="request">Receipt document creation data</param>
    /// <returns>ID of the created receipt document</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateReceipt([FromBody] CreateReceiptCommand request)
    {
        logger.LogInformation("Creating new receipt document");
        var result = await mediator.Send(request);
        logger.LogInformation("Successfully created receipt document with ID: {ReceiptId}", result);
        return CreatedAtAction(nameof(GetReceiptById), new { id = result }, result);
    }

    /// <summary>
    /// Update an existing receipt document
    /// </summary>
    /// <param name="id">The receipt document ID</param>
    /// <param name="request">Receipt document update data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReceipt([FromBody] UpdateReceiptCommand request)
    {
        logger.LogInformation("Updating receipt document with ID: {ReceiptId}", request.Id);
        await mediator.Send(request);
        logger.LogInformation("Successfully updated receipt document with ID: {ReceiptId}", request.Id);
        return NoContent();
    }

    /// <summary>
    /// Delete a receipt document
    /// </summary>
    /// <param name="id">The receipt document ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReceipt(Guid id)
    {
        logger.LogInformation("Deleting receipt document with ID: {ReceiptId}", id);
        await mediator.Send(new DeleteReceiptCommand(id));
        logger.LogInformation("Successfully deleted receipt document with ID: {ReceiptId}", id);
        return NoContent();
    }
}