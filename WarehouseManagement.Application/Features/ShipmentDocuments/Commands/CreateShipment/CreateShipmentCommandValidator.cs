using FluentValidation;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;

namespace WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

public sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("Номер документа обязателен")
            .MaximumLength(50)
            .WithMessage("Номер документа не может быть длиннее 50 символов");
        
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("ID клиента обязателен");
        
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Дата документа обязательна")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("Дата документа не может быть позже завтрашней");
        
        RuleFor(x => x.Resources)
            .NotNull()
            .WithMessage("Список ресурсов не может быть null")
            .Must(resources => resources.Any())
            .WithMessage("Документ отгрузки не может быть пустым");
        
        RuleForEach(x => x.Resources)
            .SetValidator(new ShipmentResourceDtoValidator());
    }
}

public sealed class ShipmentResourceDtoValidator : AbstractValidator<ShipmentResourceDto>
{
    public ShipmentResourceDtoValidator()
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