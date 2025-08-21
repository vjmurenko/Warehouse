using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Resources.Queries.GetResourceById;

public class GetResourceByIdHandler<T>(IBaseRepository<T> repository) : IRequestHandler<GetResourceQuery<T>, T> where T: NamedEntity
{
    public async Task<T> Handle(GetResourceQuery<T> request, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(request.Guid);
    }
}