﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public class BalanceService : IBalanceService
{
    private readonly IBalanceRepository _balanceRepository;

    public BalanceService(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task IncreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct)
    {
        var balance = await _balanceRepository.GetForUpdateAsync(resourceId, unitId, ct);

        if (balance == null)
        {
            balance = new Balance(resourceId, unitId, quantity);
            await _balanceRepository.AddAsync(balance, ct);
        }
        else
        {
            balance.Increase(quantity);
        }
    }

    public async Task DecreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct)
    {
        var balance = await _balanceRepository.GetForUpdateAsync(resourceId, unitId, ct);
        
        if (balance == null || balance.Quantity.Value < quantity.Value)
            throw new InsufficientBalanceException(
                "Resource",
                "Unit",
                quantity.Value, 
                balance?.Quantity.Value ?? 0);

        balance?.Decrease(quantity);
    }

    public async Task ValidateBalanceAvailability(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct)
    {
        var balance = await _balanceRepository.GetForUpdateAsync(resourceId, unitId, ct);
        
        if (balance == null || balance.Quantity.Value < quantity.Value)
            throw new InsufficientBalanceException(
                "Resource",
                "Unit",
                quantity.Value, 
                balance?.Quantity.Value ?? 0);
    }

    public async Task AdjustBalance(Guid resourceId, Guid unitId, decimal deltaQuantity, CancellationToken ct)
    {
        if (deltaQuantity == 0)
            return;

        var balance = await _balanceRepository.GetForUpdateAsync(resourceId, unitId, ct);

        if (deltaQuantity > 0)
        {
            var quantity = new Quantity(deltaQuantity);
            
            if (balance == null)
            {
                balance = new Balance(resourceId, unitId, quantity);
                await _balanceRepository.AddAsync(balance, ct);
            }
            else
            {
                balance.Increase(quantity);
            }
        }
        else
        {
            var decreaseAmount = new Quantity(Math.Abs(deltaQuantity));
            
            if (balance == null || balance.Quantity.Value < decreaseAmount.Value)
                throw new InsufficientBalanceException(
                    "Resource",
                    "Unit",
                    decreaseAmount.Value, 
                    balance?.Quantity.Value ?? 0);

            balance?.Decrease(decreaseAmount);
        }
    }
}