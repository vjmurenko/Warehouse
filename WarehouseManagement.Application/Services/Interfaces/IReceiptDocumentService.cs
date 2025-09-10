﻿using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IReceiptDocumentService
{
    Task ValidateReceiptRequestAsync(string number, List<ReceiptResourceDto> resources, Guid? excludeDocumentId = null, CancellationToken cancellationToken = default);
    Task ApplyReceiptBalanceChangesAsync(ReceiptDocument document, CancellationToken cancellationToken);
    Task RevertReceiptBalanceChangesAsync(ReceiptDocument document, CancellationToken cancellationToken);
    Task ApplyBalanceChangesForUpdateAsync(
        List<(Guid ResourceId, Guid UnitId, decimal Quantity)> oldResources,
        IReadOnlyCollection<ReceiptResource> newResources, 
        CancellationToken cancellationToken);
}