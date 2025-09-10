﻿﻿using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Common.Interfaces;

public interface IReceiptRepository
{
    Task<bool> ExistsByNumberAsync(string number);
    Task AddAsync(ReceiptDocument document, CancellationToken token);
    
    Task<ReceiptDocument?> GetByIdWithResourcesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReceiptDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(ReceiptDocument document, CancellationToken cancellationToken = default);

}