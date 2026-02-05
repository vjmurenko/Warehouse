using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Queries;

public record GetAllReferencesQuery<T>() : IRequest<IEnumerable<T>> where T : Reference;