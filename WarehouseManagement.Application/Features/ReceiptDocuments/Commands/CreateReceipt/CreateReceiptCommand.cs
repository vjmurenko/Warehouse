﻿using MediatR;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public record CreateReceiptCommand(
    string Number,
    DateTime Date,
    List<ReceiptResourceDto> Resources
) : IRequest<Guid>;