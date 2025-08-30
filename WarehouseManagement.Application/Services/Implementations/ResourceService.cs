using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Services.Implementations;

public class ResourceService(INamedEntityRepository<Resource> repository) : NamedEntityService<Resource>(repository), IResourceService
{
    public async Task<Guid> CreateResourceAsync(string name)
    {
        var resource = new Resource(name);
        return await CreateAsync(resource);
    }

    public async Task<bool> UpdateResourceAsync(Guid id, string name)
    {
        var resource = await GetByIdAsync(id);
        if (resource == null)
        {
            throw new EntityNotFoundException("Resource", id);
        }
        
        resource.Rename(name);
        return await UpdateAsync(resource);
    }
}