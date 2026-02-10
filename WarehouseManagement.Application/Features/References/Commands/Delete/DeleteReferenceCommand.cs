using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Commands.Delete;

public record DeleteReferenceCommand<T>(Guid Id) : IRequest where T : Reference;