using MediatR;

namespace WarehouseManagement.Application.Resources.Commands.CreateResource;

public class CreateResourceCommand : IRequest
{
    public string Name { get; set; }
}