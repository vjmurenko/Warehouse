using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Tests.TestBuilders;

namespace WarehouseManagement.Tests.Application.Features.ReceiptDocuments.Commands;

public class CreateReceiptCommandHandlerSimpleTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IBalanceService _balanceService;
    private readonly INamedEntityValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateReceiptCommandHandler _handler;

    public CreateReceiptCommandHandlerSimpleTests()
    {
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _balanceService = Substitute.For<IBalanceService>();
        _validationService = Substitute.For<INamedEntityValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new CreateReceiptCommandHandler(
            _receiptRepository,
            _balanceService,
            _validationService,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_create_receipt_when_valid_command_provided()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = TestDataBuilders.CreateReceiptCommand()
            .WithNumber("REC-001")
            .WithResources(
                TestDataBuilders.ReceiptResourceDto()
                    .WithResourceId(resourceId)
                    .WithUnitId(unitId)
                    .WithQuantity(100m)
                    .Build())
            .Build();

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        await _validationService.Received(1).ValidateResourcesAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(resourceId)),
            Arg.Any<CancellationToken>());
        
        await _validationService.Received(1).ValidateUnitsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(unitId)),
            Arg.Any<CancellationToken>());
        
        _receiptRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Any<IEnumerable<BalanceDelta>>(),
            Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_exception_when_document_number_already_exists()
    {
        // Arrange
        var command = TestDataBuilders.CreateReceiptCommand()
            .WithNumber("REC-001")
            .Build();

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
        var command = TestDataBuilders.CreateReceiptCommand()
            .WithNumber("REC-001")
            .WithResources() // Empty resources
            .Build();

        _receiptRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        _receiptRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ReceiptAggregate.ReceiptDocument>());
        
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => !deltas.Any()),
            Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}