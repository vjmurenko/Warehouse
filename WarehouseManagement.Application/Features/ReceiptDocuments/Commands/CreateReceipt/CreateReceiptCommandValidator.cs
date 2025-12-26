using FluentValidation;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;

public sealed class CreateReceiptCommandValidator : AbstractValidator<CreateReceiptCommand>
{
    public CreateReceiptCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("Номер документа обязателен")
            .MaximumLength(50)
            .WithMessage("Номер документа не может быть длиннее 50 символов");
        
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Дата документа обязательна")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("Дата документа не может быть позже завтрашней");
        
        RuleFor(x => x.Resources)
            .NotNull()
            .WithMessage("Список ресурсов не может быть null");
        
        RuleForEach(x => x.Resources)
            .SetValidator(new ReceiptResourceDtoValidator());
    }
}

public sealed class ReceiptResourceDtoValidator : AbstractValidator<ReceiptResourceDto>
{
    public ReceiptResourceDtoValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotEmpty()
            .WithMessage("ID ресурса обязателен");
        
        RuleFor(x => x.UnitId)
            .NotEmpty()
            .WithMessage("ID единицы измерения обязателен");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Количество должно быть больше 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Количество не может превышать 1,000,000");
    }
}