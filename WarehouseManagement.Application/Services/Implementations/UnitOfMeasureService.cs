using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Services.Implementations;

public class UnitOfMeasureService(INamedEntityRepository<UnitOfMeasure> repository)
    : NamedEntityService<UnitOfMeasure>(repository), IUnitOfMeasureService
{
    public async Task<Guid> CreateUnitOfMeasureAsync(string name)
    {
        var resource = new UnitOfMeasure(name);
        return await CreateAsync(resource);
    }

    public async Task<bool> UpdateUnitOfMeasureAsync(Guid id, string name)
    {
        var unitOfMeasure = await repository.GetByIdAsync(id);
        if (unitOfMeasure == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        unitOfMeasure.Rename(name);
        return await UpdateAsync(unitOfMeasure);
    }
}