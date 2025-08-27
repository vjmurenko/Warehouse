using MediatR;
using WarehouseManagement.Application.Features.Balances.DTOs;

namespace WarehouseManagement.Application.Features.Balances.Queries.GetBalances;

public record GetBalancesQuery(
    List<Guid>? ResourceIds = null,
    List<Guid>? UnitIds = null
) : IRequest<List<BalanceDto>>;