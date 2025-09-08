﻿﻿﻿using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IBalanceService
{
     Task IncreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct);
     Task DecreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct);
     Task ValidateBalanceAvailability(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct);
     Task AdjustBalance(Guid resourceId, Guid unitId, decimal deltaQuantity, CancellationToken ct);
}