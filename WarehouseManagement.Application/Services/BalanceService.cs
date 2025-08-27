using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Services;

public class BalanceService(WarehouseDbContext _context) : IBalanceService
{
    public async Task IncreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var balance = await _context.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitId\" = {1} FOR UPDATE", resourceId, unitId)
            .FirstOrDefaultAsync(ct);

        if (balance == null)
        {
            balance = new Balance(resourceId, unitId, quantity);
            _context.Balances.Add(balance);
        }
        else
        {
            balance.Increase(quantity);
        }

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
 
    
    public async Task DecreaseBalance(Guid resourceId, Guid unitId, Quantity quantity, CancellationToken ct)
    {
        
        // начинаем транзакцию
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        // блокируем строку на update
        var balance = await _context.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitId\" = {1} FOR UPDATE", resourceId, unitId)
            .FirstOrDefaultAsync(ct);

        if (balance == null || balance.Quantity.Value < quantity.Value)
            throw new Exception("Недостаточно ресурсов на складе");

        balance.Decrease(quantity);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}