using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Queries;

public record GetActiveReferencesQuery<T>() : IRequest<IEnumerable<T>> where T : Reference;