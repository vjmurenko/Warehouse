using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IUnitOfMeasureService : INamedEntityService<UnitOfMeasure>
{
    public Task<Guid> CreateUnitOfMeasureAsync(string name);
    
    public Task<bool> UpdateUnitOfMeasureAsync(Guid id, string name);
}
