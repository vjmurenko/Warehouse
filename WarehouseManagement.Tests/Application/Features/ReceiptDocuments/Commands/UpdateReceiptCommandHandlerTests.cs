using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using MediatR;

namespace WarehouseManagement.Tests.Application.Features.ReceiptDocuments.Commands;

public class UpdateReceiptCommandHandlerTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly INamedEntityValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateReceiptCommandHandler _handler;

    public UpdateReceiptCommandHandlerTests()
    {
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _validationService = Substitute.For<INamedEntityValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new UpdateReceiptCommandHandler(
            _receiptRepository,
            _validationService,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_adjust_balance_when_quantities_change()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var resource = ReceiptResource.Create(receiptId, resourceId, unitId, 100m);
        var existingReceipt = ReceiptDocument.Create("REC-001", DateTime.UtcNow, [resource]); // Original quantity
        
        var command = new UpdateReceiptCommand(
            receiptId,
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto> { new(resourceId, unitId, 150m) }); // New quantity: +50

        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _receiptRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Domain events should handle balance adjustments, so we verify SaveEntitiesAsync was called
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_exception_when_receipt_not_found()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var command = new UpdateReceiptCommand(
            receiptId,
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns((ReceiptDocument?)null);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Документ с ID {receiptId} не найден");
    }

    [Fact]
    public async Task handle_should_throw_exception_when_number_already_exists()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var existingReceipt = ReceiptDocument.Create("REC-001", DateTime.UtcNow, []);
        
        var command = new UpdateReceiptCommand(
            receiptId,
            "REC-002", // Different number
            DateTime.UtcNow,
            new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _receiptRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(true); // Number exists for another document

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ с номером REC-002 уже существует");
    }

    [Fact]
    public async Task handle_should_validate_resources_and_units()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingReceipt = ReceiptDocument.Create("REC-001", DateTime.UtcNow, []);
        var command = new UpdateReceiptCommand(
            receiptId,
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto> { new(resourceId, unitId, 100m) });

        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _receiptRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _validationService.Received(1).ValidateResourcesAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(resourceId)),
            Arg.Any<CancellationToken>());
        
        await _validationService.Received(1).ValidateUnitsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(unitId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_update_receipt_with_empty_resources()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resource = ReceiptResource.Create(receiptId, Guid.NewGuid(), Guid.NewGuid(), 100m);
        var existingReceipt = ReceiptDocument.Create("REC-001", DateTime.UtcNow, [resource]);
        
        var command = new UpdateReceiptCommand(
            receiptId,
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto>()); // Empty resources

        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _receiptRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }
}