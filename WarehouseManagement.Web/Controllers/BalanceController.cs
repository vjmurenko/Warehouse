using Microsoft.AspNetCore.Mvc;
using MediatR;
using WarehouseManagement.Application.Features.Balances.Queries.GetBalances;
using WarehouseManagement.Application.Features.Balances.DTOs;

namespace WarehouseManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanceController(IMediator mediator, ILogger<BalanceController> logger) : ControllerBase
{
    /// <summary>
    /// Get all balances with optional filtering by resource IDs and unit of measure IDs
    /// </summary>
    /// <param name="resourceIds">Optional list of resource IDs to filter by</param>
    /// <param name="unitIds">Optional list of unit of measure IDs to filter by</param>
    /// <returns>List of balance DTOs</returns>
    [HttpGet]
    public async Task<ActionResult<List<BalanceDto>>> GetBalances(
        [FromQuery] List<Guid>? resourceIds = null,
        [FromQuery] List<Guid>? unitIds = null)
    {
        logger.LogInformation("Getting balances with {ResourceCount} resource filters and {UnitCount} unit filters",
            resourceIds?.Count ?? 0, unitIds?.Count ?? 0);
        
        var query = new GetBalancesQuery(resourceIds, unitIds);
        var result = await mediator.Send(query);
        
        logger.LogInformation("Successfully retrieved {Count} balances", result.Count);
        return Ok(result);
    }
}