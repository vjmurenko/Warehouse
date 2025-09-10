using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Application.Services.Interfaces;

/// <summary>
/// Service responsible for complex document querying and filtering logic.
/// Separated from repositories to maintain Single Responsibility Principle.
/// </summary>
public interface IDocumentQueryService
{
    /// <summary>
    /// Get filtered receipt documents with complex filtering logic
    /// </summary>
    /// <param name="fromDate">Filter documents from this date</param>
    /// <param name="toDate">Filter documents to this date</param>
    /// <param name="documentNumbers">Filter by specific document numbers</param>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of filtered receipt documents with resources</returns>
    Task<List<ReceiptDocument>> GetFilteredReceiptsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get filtered shipment documents with complex filtering logic
    /// </summary>
    /// <param name="fromDate">Filter documents from this date</param>
    /// <param name="toDate">Filter documents to this date</param>
    /// <param name="documentNumbers">Filter by specific document numbers</param>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of filtered shipment documents with resources</returns>
    Task<List<ShipmentDocument>> GetFilteredShipmentsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        List<string>? documentNumbers = null,
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get filtered balances with complex filtering logic
    /// </summary>
    /// <param name="resourceIds">Filter by specific resource IDs</param>
    /// <param name="unitIds">Filter by specific unit of measure IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of filtered balances</returns>
    Task<List<Balance>> GetFilteredBalancesAsync(
        List<Guid>? resourceIds = null,
        List<Guid>? unitIds = null,
        CancellationToken cancellationToken = default);
}