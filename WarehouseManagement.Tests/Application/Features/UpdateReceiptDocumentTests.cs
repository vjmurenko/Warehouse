using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.UpdateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class UpdateReceiptDocumentTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly INamedEntityValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly UpdateReceiptCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly ReceiptDocument _defaultReceiptDocument;
    
    public UpdateReceiptDocumentTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _validationService = Substitute.For<INamedEntityValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new UpdateReceiptCommandHandler(_receiptRepository, _balanceService, _validationService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
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
        _receiptRepository.ExistsByNumberAsync(newNumber, _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>())
            .Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>())
            .Returns(_defaultUnitOfMeasure);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).IncreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            newQuantity, Arg.Any<CancellationToken>());
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
    public async Task UpdateReceiptDocument_WithDuplicateNumber_ShouldThrowException()
    {
        // Arrange
        var duplicateNumber = "DUPLICATE-123";
        var command = new UpdateReceiptCommand(_defaultDocumentId, duplicateNumber, DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.ExistsByNumberAsync(duplicateNumber, _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с номером {duplicateNumber} уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var invalidResourceId = Guid.NewGuid();
        var receiptDto = new ReceiptResourceDto(invalidResourceId, _defaultUnitOfMeasureId, 10);
        var command = new UpdateReceiptCommand(_defaultDocumentId, "TEST-123", DateTime.Now, new List<ReceiptResourceDto> { receiptDto });

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.ExistsByNumberAsync("TEST-123", _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _validationService.ValidateResourceAsync(invalidResourceId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {invalidResourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {invalidResourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithResourceChanges_ShouldUpdateBalanceCorrectly()
    {
        // Arrange
        var oldResource1Id = _defaultResourceId;
        var oldUnit1Id = _defaultUnitOfMeasureId;
        var newResource2Id = Guid.NewGuid();
        var newUnit2Id = Guid.NewGuid();
        
        var newReceipts = new List<ReceiptResourceDto>
        {
            new(newResource2Id, newUnit2Id, 15) // Completely different resource
        };
        
        var command = new UpdateReceiptCommand(_defaultDocumentId, "NEW-123", DateTime.Now, newReceipts);
        var newResource2 = new Resource("New Resource") { Id = newResource2Id };
        var newUnit2 = new UnitOfMeasure("New Unit") { Id = newUnit2Id };

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.ExistsByNumberAsync("NEW-123", _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _validationService.ValidateResourceAsync(newResource2Id, Arg.Any<CancellationToken>())
            .Returns(newResource2);
        _validationService.ValidateUnitOfMeasureAsync(newUnit2Id, Arg.Any<CancellationToken>())
            .Returns(newUnit2);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Should decrease old resource balance
        await _balanceService.Received(1).DecreaseBalance(oldResource1Id, oldUnit1Id, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        
        // Should increase new resource balance
        await _balanceService.Received(1).IncreaseBalance(newResource2Id, newUnit2Id, 
            Arg.Is<Quantity>(q => q.Value == 15), Arg.Any<CancellationToken>());
        
        await _receiptRepository.Received(1).UpdateAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithMultipleResources_ShouldHandleCorrectly()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var newReceipts = new List<ReceiptResourceDto>
        {
            new(resource1Id, unit1Id, 20),
            new(resource2Id, unit2Id, 30)
        };
        
        var command = new UpdateReceiptCommand(_defaultDocumentId, "MULTI-123", DateTime.Now, newReceipts);
        var resource1 = new Resource("Resource1") { Id = resource1Id };
        var resource2 = new Resource("Resource2") { Id = resource2Id };
        var unit1 = new UnitOfMeasure("Unit1") { Id = unit1Id };
        var unit2 = new UnitOfMeasure("Unit2") { Id = unit2Id };

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.ExistsByNumberAsync("MULTI-123", _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _validationService.ValidateResourceAsync(resource1Id, Arg.Any<CancellationToken>())
            .Returns(resource1);
        _validationService.ValidateResourceAsync(resource2Id, Arg.Any<CancellationToken>())
            .Returns(resource2);
        _validationService.ValidateUnitOfMeasureAsync(unit1Id, Arg.Any<CancellationToken>())
            .Returns(unit1);
        _validationService.ValidateUnitOfMeasureAsync(unit2Id, Arg.Any<CancellationToken>())
            .Returns(unit2);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Should decrease old resource balance
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        
        // Should increase new resources balances
        await _balanceService.Received(1).IncreaseBalance(resource1Id, unit1Id, 
            Arg.Is<Quantity>(q => q.Value == 20), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).IncreaseBalance(resource2Id, unit2Id, 
            Arg.Is<Quantity>(q => q.Value == 30), Arg.Any<CancellationToken>());
        
        await _receiptRepository.Received(1).UpdateAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReceiptDocument_WithEmptyResources_ShouldClearAllResources()
    {
        // Arrange
        var command = new UpdateReceiptCommand(_defaultDocumentId, "EMPTY-123", DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _receiptRepository.ExistsByNumberAsync("EMPTY-123", _defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Should decrease old resource balance
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        
        // Should not increase any balance
        await _balanceService.DidNotReceive().IncreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), 
            Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        
        await _receiptRepository.Received(1).UpdateAsync(_defaultReceiptDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}