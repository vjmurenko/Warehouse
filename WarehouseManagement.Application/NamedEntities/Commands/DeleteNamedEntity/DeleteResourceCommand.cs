using MediatR;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Commands.DeleteResource;

public class DeleteResourceCommand : IRequest<bool>
{
    public Resource Resource { get; set; }
}