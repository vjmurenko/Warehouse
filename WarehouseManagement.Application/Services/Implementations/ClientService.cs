using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Services.Implementations;

public class ClientService(INamedEntityRepository<Client> repository) : NamedEntityService<Client>(repository), IClientService
{
    public async Task<Guid> CreateClientAsync(string name, string addressName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        var client = new Client(name, addressName);
        return await CreateAsync(client);
    }

    public async Task<bool> UpdateClientAsync(Guid id, string name, string addressName)
    {
        var client = await GetByIdAsync(id);
        if (client == null) return false;

        client.Rename(name);
        client.ChangeAddress(addressName);

        return await UpdateAsync(client);
    }
}
