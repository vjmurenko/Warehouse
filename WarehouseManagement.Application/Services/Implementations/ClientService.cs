using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class ClientService(INamedEntityRepository<Client> repository, IUnitOfWork unitOfWork) : NamedEntityService<Client>(repository, unitOfWork), IClientService
{
    public async Task<Guid> CreateClientAsync(string name, string addressName, CancellationToken ctx)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        var client = new Client(name, addressName);
        return await CreateAsync(client, ctx);
    }

    public async Task<bool> UpdateClientAsync(Guid id, string name, string addressName, CancellationToken ctx)
    {
        var client = await GetByIdAsync(id, ctx);
        if (client == null) return false;

        client.Rename(name);
        client.ChangeAddress(addressName);

        return await UpdateAsync(client, ctx);
    }
}
