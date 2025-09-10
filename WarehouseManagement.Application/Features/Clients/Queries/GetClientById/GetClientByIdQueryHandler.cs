using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQueryHandler(IClientService clientService) : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    public async Task<ClientDto?> Handle(GetClientByIdQuery query, CancellationToken cancellationToken)
    {
        var client = await clientService.GetByIdAsync(query.Id);
        
        if (client == null)
            return null;
        
        return new ClientDto(
            client.Id,
            client.Name,
            client.Address.Name,
            client.IsActive
        );
    }
}