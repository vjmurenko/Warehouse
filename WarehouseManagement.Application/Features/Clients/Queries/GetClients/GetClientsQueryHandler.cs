using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetClients;

public class GetClientsQueryHandler(IClientService clientService) : IRequestHandler<GetClientsQuery, List<ClientDto>>
{
    public async Task<List<ClientDto>> Handle(GetClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = await clientService.GetAllAsync();
        return clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Address.Name,
            c.IsActive
        )).ToList();
    }
}