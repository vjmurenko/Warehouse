using FluentValidation;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;

public sealed class DeleteReceiptCommandValidator : AbstractValidator<DeleteReceiptCommand>
{
    public DeleteReceiptCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID документа обязателен");
    }
}