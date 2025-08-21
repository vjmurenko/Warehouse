using MediatR;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Resources.Queries.GetResourceById;

public class GetResourceQuery<T> : IRequest<T> where T : NamedEntity
{
    public Guid Guid { get; set; }
}