using MediatR;
using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Application.Features.References.Queries;

public record GetReferenceByIdQuery<T>(Guid Id) : IRequest<T> where T : Reference; 