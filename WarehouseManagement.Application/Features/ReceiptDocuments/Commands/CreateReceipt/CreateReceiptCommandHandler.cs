﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public class CreateReceiptCommandHandler(
    IReceiptRepository receiptRepository,
    IReceiptDocumentService receiptDocumentService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReceiptCommand, Guid>
{
    public async Task<Guid> Handle(CreateReceiptCommand command, CancellationToken cancellationToken)
    {
        await receiptDocumentService.ValidateReceiptRequestAsync(command.Number, command.Resources, cancellationToken: cancellationToken);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var receiptDocument = new ReceiptDocument(command.Number, command.Date);
            foreach (var dto in command.Resources)
            {
                receiptDocument.AddResource(dto.ResourceId, dto.UnitId, dto.Quantity);
            }

            await receiptRepository.AddAsync(receiptDocument, cancellationToken);

            await receiptDocumentService.ApplyReceiptBalanceChangesAsync(receiptDocument, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return receiptDocument.Id;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}