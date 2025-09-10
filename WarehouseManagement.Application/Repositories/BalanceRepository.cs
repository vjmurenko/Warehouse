﻿﻿﻿﻿﻿﻿﻿using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Repositories;

public class BalanceRepository(WarehouseDbContext context) : RepositoryBase<Balance>(context), IBalanceRepository
{
    public async Task<Balance?> GetForUpdateAsync(Guid resourceId, Guid unitId, CancellationToken ct)
    {
        return await DbContext.Balances
            .FromSqlRaw("SELECT * FROM \"Balances\" WHERE \"ResourceId\" = {0} AND \"UnitOfMeasureId\" = {1} FOR UPDATE",
                resourceId, unitId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Balance balance, CancellationToken ct)
    {
        await DbContext.Balances.AddAsync(balance, ct);
    }
}