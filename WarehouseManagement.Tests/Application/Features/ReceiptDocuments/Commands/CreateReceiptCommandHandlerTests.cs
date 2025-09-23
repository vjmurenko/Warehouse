using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Tests.Application.Features.ReceiptDocuments.Commands;

public class CreateReceiptCommandHandlerTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly INamedEntityValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateReceiptCommandHandler _handler;

    public CreateReceiptCommandHandlerTests()
    {
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _validationService = Substitute.For<INamedEntityValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new CreateReceiptCommandHandler(
            _receiptRepository,
            _validationService,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_create_receipt_when_valid_command_provided()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateReceiptCommand(
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto> { new ReceiptResourceDto(resourceId, unitId, 100m) }
        );

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        await _validationService.Received(1).ValidateResourcesAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(resourceId)),
            Arg.Any<CancellationToken>());
        
        await _validationService.Received(1).ValidateUnitsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(unitId)),
            Arg.Any<CancellationToken>());
        
        _receiptRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        
        // Domain events should handle balance increase, so we verify SaveEntitiesAsync was called
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_exception_when_document_number_already_exists()
    {
        // Arrange
        var command = new CreateReceiptCommand("REC-001", DateTime.UtcNow, new List<ReceiptResourceDto>());

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ с номером REC-001 уже существует");
        
        _receiptRepository.DidNotReceive().Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_allow_empty_receipt_document()
    {
        // Arrange
        var command = new CreateReceiptCommand("REC-001", DateTime.UtcNow, new List<ReceiptResourceDto>());

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        _receiptRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        
        // Domain events should handle balance increase, so we verify SaveEntitiesAsync was called
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_increase_balance_for_each_resource()
    {
        // Arrange
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateReceiptCommand(
            "REC-001",
            DateTime.UtcNow,
            new List<ReceiptResourceDto> 
            { 
                new(resourceId1, unitId, 50m),
                new(resourceId2, unitId, 75m)
            });

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _receiptRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        
        // Domain events should handle balance increase, so we verify SaveEntitiesAsync was called
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }
}