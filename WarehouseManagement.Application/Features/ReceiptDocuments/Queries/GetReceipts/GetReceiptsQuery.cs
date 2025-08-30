using MediatR;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;

public record GetReceiptsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    List<string>? DocumentNumbers = null,
    List<Guid>? ResourceIds = null,
    List<Guid>? UnitIds = null
) : IRequest<List<ReceiptDocumentDto>>;