using MediatR;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public record DeleteReceiptCommand(Guid Id) : IRequest<Unit>;