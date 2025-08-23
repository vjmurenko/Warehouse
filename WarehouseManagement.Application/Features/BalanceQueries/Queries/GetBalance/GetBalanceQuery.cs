using MediatR;
using WarehouseManagement.Application.Features.BalanceQueries.DTOs;

namespace WarehouseManagement.Application.Features.BalanceQueries.Queries.GetBalance;

public record GetBalanceQuery(
    List<Guid>? ResourceIds = null,
    List<Guid>? UnitOfMeasureIds = null
) : IRequest<List<BalanceDto>>;
