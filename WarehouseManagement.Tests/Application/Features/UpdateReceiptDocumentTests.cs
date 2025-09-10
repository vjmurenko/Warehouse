using MediatR;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;
using Xunit;

namespace WarehouseManagement.Tests.Application.Features;

public class UpdateReceiptDocumentTests
{
    // Mocks and test infrastructure
    private readonly IReceiptRepository _receiptRepository;
    private readonly IReceiptDocumentService _receiptDocumentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateReceiptCommandHandler _handler;

    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly ReceiptDocument _defaultReceiptDocument;

    public UpdateReceiptDocumentTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _receiptDocumentService = Substitute.For<IReceiptDocumentService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        // Initialize handler
        _handler = new UpdateReceiptCommandHandler(_receiptRepository, _receiptDocumentService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultReceiptDocument = new ReceiptDocument("OLD-123", DateTime.Now.AddDays(-1));
        _defaultReceiptDocument.GetType().GetProperty("Id")?.SetValue(_defaultReceiptDocument, _defaultDocumentId);
        _defaultReceiptDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 10);
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var newQuantity = new Quantity(25);
        var newNumber = "UPD-123";
        var newDate = DateTime.Now;

        var receiptDto = new ReceiptResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, newQuantity.Value);
        var command = new UpdateReceiptCommand(_defaultDocumentId, newNumber, newDate, new List<ReceiptResourceDto> { receiptDto });

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _receiptDocumentService.Received(1).ValidateReceiptRequestAsync(newNumber, command.Resources, _defaultDocumentId, Arg.Any<CancellationToken>());
        await _receiptDocumentService.Received(1).ApplyBalanceChangesForUpdateAsync(Arg.Any<List<(Guid ResourceId, Guid UnitId, decimal Quantity)>>(), Arg.Any<IReadOnlyCollection<ReceiptResource>>(), Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).UpdateAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithNonExistentDocument_ShouldThrowException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateReceiptCommand(nonExistentId, "TEST-123", DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((ReceiptDocument?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с ID {nonExistentId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WorkflowTest_ShouldCallCorrectServices()
    {
        // Arrange
        var command = new UpdateReceiptCommand(_defaultDocumentId, "WORKFLOW-123", DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Focus on SRP: handler orchestrates, service handles details
        await _receiptDocumentService.Received(1).ValidateReceiptRequestAsync("WORKFLOW-123", command.Resources, _defaultDocumentId, Arg.Any<CancellationToken>());
        await _receiptDocumentService.Received(1).ApplyBalanceChangesForUpdateAsync(Arg.Any<List<(Guid ResourceId, Guid UnitId, decimal Quantity)>>(), Arg.Any<IReadOnlyCollection<ReceiptResource>>(), Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).UpdateAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}