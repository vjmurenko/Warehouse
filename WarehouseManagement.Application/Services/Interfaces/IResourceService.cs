using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IResourceService : INamedEntityService<Resource>
{
    Task<Guid> CreateResourceAsync(string name);
    
    Task<bool> UpdateResourceAsync(Guid id, string name);
}
