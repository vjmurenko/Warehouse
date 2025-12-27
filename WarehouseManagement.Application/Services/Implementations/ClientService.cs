using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Implementations;

public sealed class ClientService(INamedEntityRepository<Client> repository, IUnitOfWork unitOfWork, ILogger<ClientService> logger)
    : NamedEntityService<Client>(repository, unitOfWork, logger), IClientService
{
    public async Task<Guid> CreateClientAsync(string name, string addressName, CancellationToken ctx)
    {
        logger.LogInformation("Creating client with name: {ClientName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            logger.LogWarning("Attempted to create client with empty name");
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }
        
        var address = new Address(addressName);
        var client = new Client(name, address);
        var result = await CreateAsync(client, ctx);
        
        logger.LogInformation("Successfully created client with ID: {ClientId}", result);
        return result;
    }

    public async Task<bool> UpdateClientAsync(Guid id, string name, string addressName, CancellationToken ctx)
    {
        logger.LogInformation("Updating client with ID: {ClientId}", id);
        
        var client = await GetByIdAsync(id, ctx);
        if (client is null)
        {
            logger.LogWarning("Client with ID: {ClientId} not found for update", id);
            return false;
        }

        client.Rename(name);
        client.ChangeAddress(new Address(addressName));

        var result = await UpdateAsync(client, ctx);
        
        logger.LogInformation("Successfully updated client with ID: {ClientId}", id);
        return result;
    }
}
