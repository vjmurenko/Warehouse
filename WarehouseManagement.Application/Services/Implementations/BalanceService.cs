﻿﻿﻿using WarehouseManagement.Application.Common.Interfaces;
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
                "Resource", // We'll need to get actual names later
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
                "Resource", // We'll need to get actual names later
                "Unit",
                quantity.Value, 
                balance?.Quantity.Value ?? 0);
        
        // Не изменяем баланс, только проверяем доступность
    }
}