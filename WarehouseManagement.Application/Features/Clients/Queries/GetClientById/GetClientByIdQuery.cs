using MediatR;
using WarehouseManagement.Application.Features.Clients.DTOs;

namespace WarehouseManagement.Application.Features.Clients.Queries.GetClientById;

public record GetClientByIdQuery(Guid Id) : IRequest<ClientDto?>;