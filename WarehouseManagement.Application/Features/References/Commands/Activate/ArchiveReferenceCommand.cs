using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Commands;

public record ArchiveReferenceCommand<T>(Guid Id) : IRequest where T : Reference;
