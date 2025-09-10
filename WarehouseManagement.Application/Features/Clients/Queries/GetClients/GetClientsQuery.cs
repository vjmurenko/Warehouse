using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetClients;

public record GetClientsQuery() : IRequest<List<ClientDto>>;