using MediatR;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;

public record UpdateReceiptCommand(
    Guid Id,
    string Number,
    DateTime Date,
    List<ReceiptResourceDto> Resources
) : IRequest<Unit>;