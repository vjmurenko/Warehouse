using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Application.Services.Implementations;

public class ClientService(INamedEntityRepository<Client> repository) : NamedEntityService<Client>(repository), IClientService
{
    public async Task<Guid> CreateClientAsync(string name, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (address == null)
            throw new ArgumentNullException(nameof(address));

        var client = new Client(name, address);
        return await CreateAsync(client);
    }

    public async Task<bool> UpdateClientAsync(Guid id, string name, Address address)
    {
        var client = await GetByIdAsync(id);
        if (client == null) return false;

        client.Rename(name);
        client.ChangeAddress(address);

        return await UpdateAsync(client);
    }
}
