using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetActiveClients;

public record GetActiveClientsQuery() : IRequest<List<ClientDto>>;