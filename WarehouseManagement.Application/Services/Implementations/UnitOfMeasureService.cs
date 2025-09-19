using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class UnitOfMeasureService(INamedEntityRepository<UnitOfMeasure> repository, IUnitOfWork unitOfWork, ILogger<UnitOfMeasureService> logger)
    : NamedEntityService<UnitOfMeasure>(repository, unitOfWork, logger), IUnitOfMeasureService
{
    public async Task<Guid> CreateUnitOfMeasureAsync(string name, CancellationToken ctx)
    {
        var resource = new UnitOfMeasure(name);
        return await CreateAsync(resource, ctx);
    }

    public async Task<bool> UpdateUnitOfMeasureAsync(Guid id, string name, CancellationToken ctx)
    {
        var unitOfMeasure = await repository.GetByIdAsync(id, ctx);
        if (unitOfMeasure == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        unitOfMeasure.Rename(name);
        return await UpdateAsync(unitOfMeasure, ctx);
    }
}