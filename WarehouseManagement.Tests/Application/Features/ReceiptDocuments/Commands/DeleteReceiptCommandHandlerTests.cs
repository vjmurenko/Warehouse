using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Exceptions;
using MediatR;

namespace WarehouseManagement.Tests.Application.Features.ReceiptDocuments.Commands;

public class DeleteReceiptCommandHandlerTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteReceiptCommandHandler _handler;

    public DeleteReceiptCommandHandlerTests()
    {
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new DeleteReceiptCommandHandler(
            _receiptRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_decrease_balance_for_each_resource()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingReceipt = new ReceiptDocument("REC-001", DateTime.UtcNow);
        existingReceipt.AddResource(resourceId, unitId, 100m);
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 1 &&
                deltas.First().ResourceId == resourceId &&
                deltas.First().Quantity == 100m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_exception_when_receipt_not_found()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns((ReceiptDocument?)null);

        // Act
        var action = async () => await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"ReceiptDocument with ID {receiptId} was not found");
    }

    [Fact]
    public async Task handle_should_throw_when_insufficient_balance()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingReceipt = new ReceiptDocument("REC-001", DateTime.UtcNow);
        existingReceipt.AddResource(resourceId, unitId, 100m);
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        
        _balanceService.DecreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Insufficient balance")));

        // Act
        var action = async () => await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient balance");
    }

    [Fact]
    public async Task handle_should_delete_receipt_and_save_changes()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var existingReceipt = new ReceiptDocument("REC-001", DateTime.UtcNow);
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _receiptRepository.Received(1).Delete(existingReceipt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_handle_receipt_with_multiple_resources()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingReceipt = new ReceiptDocument("REC-001", DateTime.UtcNow);
        existingReceipt.AddResource(resourceId1, unitId, 50m);
        existingReceipt.AddResource(resourceId2, unitId, 75m);
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 2 &&
                deltas.Any(d => d.ResourceId == resourceId1 && d.Quantity == 50m) &&
                deltas.Any(d => d.ResourceId == resourceId2 && d.Quantity == 75m)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_handle_empty_receipt()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var existingReceipt = new ReceiptDocument("REC-001", DateTime.UtcNow);
        // No resources added
        
        _receiptRepository.GetByIdWithResourcesAsync(receiptId, Arg.Any<CancellationToken>())
            .Returns(existingReceipt);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(new DeleteReceiptCommand(receiptId), CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => !deltas.Any()),
            Arg.Any<CancellationToken>());
        
        _receiptRepository.Received(1).Delete(existingReceipt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}