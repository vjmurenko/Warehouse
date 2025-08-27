using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;

public class GetReceiptsQueryHandler(
    IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptsQuery, List<ReceiptDocumentSummaryDto>>
{
    public async Task<List<ReceiptDocumentSummaryDto>> Handle(GetReceiptsQuery query, CancellationToken cancellationToken)
    {
        var documents = await receiptRepository.GetFilteredAsync(
            query.FromDate,
            query.ToDate,
            query.DocumentNumbers,
            query.ResourceIds,
            query.UnitIds,
            cancellationToken);

        return documents.Select(doc => new ReceiptDocumentSummaryDto(
            doc.Id,
            doc.Number,
            doc.Date,
            doc.ReceiptResources.Count
        )).ToList();
    }
}