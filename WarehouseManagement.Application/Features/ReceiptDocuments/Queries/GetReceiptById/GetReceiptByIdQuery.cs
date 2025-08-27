using MediatR;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;

public record GetReceiptByIdQuery(Guid Id) : IRequest<ReceiptDocumentDto?>;