using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Application.Services.Interfaces;

public interface IClientService : INamedEntityService<Client>
{
    Task<Guid> CreateClientAsync(string name, string addressName, CancellationToken ctx);
    Task<bool> UpdateClientAsync(Guid id, string name, string addressName, CancellationToken ctx);
}
