using MediatR;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Application.Resources.Commands.UpdateResource;

public class UpdateResourceCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Resource Resource { get; set; }
}