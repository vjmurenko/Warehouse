using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetActiveClients;

public class GetActiveClientsQueryHandler(IClientService clientService) : IRequestHandler<GetActiveClientsQuery, List<ClientDto>>
{
    public async Task<List<ClientDto>> Handle(GetActiveClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = await clientService.GetActiveAsync();
        return clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Address.Name,
            c.IsActive
        )).ToList();
    }
}