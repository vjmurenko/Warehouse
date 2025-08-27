using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class DeleteReceiptDocumentTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly DeleteReceiptCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly ReceiptDocument _defaultReceiptDocument;
    
    public DeleteReceiptDocumentTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new DeleteReceiptCommandHandler(_receiptRepository, _balanceService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultReceiptDocument = new ReceiptDocument("DEL-123", DateTime.Now.AddDays(-1));
        _defaultReceiptDocument.GetType().GetProperty("Id")?.SetValue(_defaultReceiptDocument, _defaultDocumentId);
        _defaultReceiptDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithValidDocument_ShouldDeleteSuccessfully()
    {
        // Arrange
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 15), Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).DeleteAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithNonExistentDocument_ShouldThrowException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteReceiptCommand(nonExistentId);

        _receiptRepository.GetByIdWithResourcesAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((ReceiptDocument?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с ID {nonExistentId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, Arg.Any<Quantity>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурса для удаления документа"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурса для удаления документа", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _receiptRepository.DidNotReceive().DeleteAsync(Arg.Any<ReceiptDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithMultipleResources_ShouldDecreaseAllBalances()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var multiResourceDocument = new ReceiptDocument("MULTI-DEL-123", DateTime.Now.AddDays(-1));
        multiResourceDocument.GetType().GetProperty("Id")?.SetValue(multiResourceDocument, _defaultDocumentId);
        multiResourceDocument.AddResource(resource1Id, unit1Id, 10);
        multiResourceDocument.AddResource(resource2Id, unit2Id, 20);
        
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(multiResourceDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalance(resource1Id, unit1Id, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).DecreaseBalance(resource2Id, unit2Id, 
            Arg.Is<Quantity>(q => q.Value == 20), Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).DeleteAsync(multiResourceDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithEmptyDocument_ShouldDeleteWithoutBalanceChanges()
    {
        // Arrange
        var emptyDocument = new ReceiptDocument("EMPTY-DEL-123", DateTime.Now.AddDays(-1));
        emptyDocument.GetType().GetProperty("Id")?.SetValue(emptyDocument, _defaultDocumentId);
        
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(emptyDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).DeleteAsync(emptyDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WhenBalanceUpdateFails_ShouldRollback()
    {
        // Arrange
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, Arg.Any<Quantity>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Balance update failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Balance update failed", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _receiptRepository.DidNotReceive().DeleteAsync(Arg.Any<ReceiptDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WhenRepositoryDeleteFails_ShouldRollback()
    {
        // Arrange
        var command = new DeleteReceiptCommand(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.DeleteAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Database error", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}