using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Application.Services.Implementations;

public class ResourceService(INamedEntityRepository<Resource> repository, IUnitOfWork unitOfWork, ILogger<ResourceService> logger) : NamedEntityService<Resource>(repository, unitOfWork, logger), IResourceService
{
    public async Task<Guid> CreateResourceAsync(string name, CancellationToken ctx)
    {
        var resource = new Resource(name);
        return await CreateAsync(resource, ctx);
    }

    public async Task<bool> UpdateResourceAsync(Guid id, string name, CancellationToken ctx)
    {
        var resource = await GetByIdAsync(id, ctx);
        if (resource == null)
        {
            throw new EntityNotFoundException("Resource", id);
        }
        
        resource.Rename(name);
        return await UpdateAsync(resource, ctx);
    }
}