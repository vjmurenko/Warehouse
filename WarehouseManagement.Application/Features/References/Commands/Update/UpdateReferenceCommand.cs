using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Commands.Update;

public record UpdateReferenceCommand<T>(Guid Id, string Name) : IRequest where T : Reference;