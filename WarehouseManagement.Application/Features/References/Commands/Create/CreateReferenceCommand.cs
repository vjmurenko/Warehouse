using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Commands.Create;

public record CreateReferenceCommand<T>(string Name) : IRequest<Guid> where T : Reference; 
