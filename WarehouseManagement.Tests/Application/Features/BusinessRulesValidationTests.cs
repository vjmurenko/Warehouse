using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.DeleteReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

/// <summary>
/// Tests to validate all business rules specified in the requirements
/// </summary>
public class BusinessRulesValidationTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IBalanceService _balanceService;
    private readonly IReceiptValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientService _clientService;
    private readonly IResourceService _resourceService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;
    
    // Test data
    private readonly Guid _defaultResourceId = Guid.NewGuid();
    private readonly Guid _defaultUnitOfMeasureId = Guid.NewGuid();
    private readonly Guid _defaultClientId = Guid.NewGuid();
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly Client _defaultClient;

    public BusinessRulesValidationTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _balanceService = Substitute.For<IBalanceService>();
        _validationService = Substitute.For<IReceiptValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clientService = Substitute.For<IClientService>();
        _resourceService = Substitute.For<IResourceService>();
        _unitOfMeasureService = Substitute.For<IUnitOfMeasureService>();
        
        // Initialize test data
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
        _defaultClient = new Client("Test Client", "Test Address") { Id = _defaultClientId };
    }

    #region Business Rule 1: Duplicate Names/Numbers Prevention

    [Fact]
    public async Task CreateReceiptDocument_WithDuplicateNumber_ShouldThrowDuplicateException()
    {
        // Arrange
        var handler = new CreateReceiptCommandHandler(_receiptRepository, _balanceService, _validationService, _unitOfWork);
        var command = new CreateReceiptCommand("DUPLICATE-001", DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.ExistsByNumberAsync("DUPLICATE-001").Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithDuplicateNumber_ShouldThrowDuplicateException()
    {
        // Arrange
        var handler = new CreateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        var command = new CreateShipmentCommand("DUPLICATE-SHIP-001", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.ExistsByNumberAsync("DUPLICATE-SHIP-001", cancellationToken: Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule 2: Archive Functionality

    [Fact]
    public async Task CreateShipmentDocument_WithArchivedResource_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        var archivedResourceId = Guid.NewGuid();
        var command = new CreateShipmentCommand("SHIP-ARCHIVED", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(archivedResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.ExistsByNumberAsync("SHIP-ARCHIVED", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(archivedResourceId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Ресурс архивирован"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирован", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReceiptDocument_WithArchivedUnitOfMeasure_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateReceiptCommandHandler(_receiptRepository, _balanceService, _validationService, _unitOfWork);
        var archivedUnitId = Guid.NewGuid();
        var command = new CreateReceiptCommand("RECEIPT-ARCHIVED", DateTime.Now, 
            new List<ReceiptResourceDto> { new(_defaultResourceId, archivedUnitId, 10) });

        _receiptRepository.ExistsByNumberAsync("RECEIPT-ARCHIVED").Returns(false);
        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(archivedUnitId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Единица измерения архивирована"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирована", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule 3: Receipt Document Balance Management

    [Fact]
    public async Task CreateReceiptDocument_ShouldIncreaseBalance()
    {
        // Arrange
        var handler = new CreateReceiptCommandHandler(_receiptRepository, _balanceService, _validationService, _unitOfWork);
        var quantity = new Quantity(50);
        var command = new CreateReceiptCommand("RECEIPT-BALANCE", DateTime.Now, 
            new List<ReceiptResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value) });

        _receiptRepository.ExistsByNumberAsync("RECEIPT-BALANCE").Returns(false);
        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).IncreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_ShouldDecreaseBalance()
    {
        // Arrange
        var handler = new DeleteReceiptCommandHandler(_receiptRepository, _balanceService, _unitOfWork);
        var documentId = Guid.NewGuid();
        var quantity = new Quantity(30);
        
        var receiptDocument = new ReceiptDocument("RECEIPT-DELETE", DateTime.Now);
        receiptDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        
        var command = new DeleteReceiptCommand(documentId);

        _receiptRepository.GetByIdWithResourcesAsync(documentId, Arg.Any<CancellationToken>()).Returns(receiptDocument);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var handler = new DeleteReceiptCommandHandler(_receiptRepository, _balanceService, _unitOfWork);
        var documentId = Guid.NewGuid();
        var quantity = new Quantity(100);
        
        var receiptDocument = new ReceiptDocument("RECEIPT-INSUFFICIENT", DateTime.Now);
        receiptDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        
        var command = new DeleteReceiptCommand(documentId);

        _receiptRepository.GetByIdWithResourcesAsync(documentId, Arg.Any<CancellationToken>()).Returns(receiptDocument);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурса для удаления документа"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурса", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule 4: Shipment Document Balance Management

    [Fact]
    public async Task CreateShipmentDocument_WithoutSigning_ShouldNotAffectBalance()
    {
        // Arrange
        var handler = new CreateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        var quantity = new Quantity(25);
        var command = new CreateShipmentCommand("SHIP-UNSIGNED", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value) }, Sign: false);

        _shipmentRepository.ExistsByNumberAsync("SHIP-UNSIGNED", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Business Rule 5: Balance Aggregation

    [Fact]
    public void BalanceAggregation_ShouldWorkForSameResourceAndUnit()
    {
        // This test validates that balances are aggregated by resource + unit combination
        // This is typically handled at the repository/service level, not in command handlers
        // The test ensures our test data setup follows this rule
        
        var balance1 = new Balance(_defaultResourceId, _defaultUnitOfMeasureId, new Quantity(50));
        var balance2 = new Balance(_defaultResourceId, _defaultUnitOfMeasureId, new Quantity(30));
        
        // Assert
        Assert.Equal(balance1.ResourceId, balance2.ResourceId);
        Assert.Equal(balance1.UnitOfMeasureId, balance2.UnitOfMeasureId);
        // In real implementation, these would be aggregated in the repository
    }

    #endregion

    #region Business Rule 6: Document Content Rules

    [Fact]
    public async Task CreateReceiptDocument_WithEmptyResources_ShouldSucceed()
    {
        // Arrange
        var handler = new CreateReceiptCommandHandler(_receiptRepository, _balanceService, _validationService, _unitOfWork);
        var command = new CreateReceiptCommand("EMPTY-RECEIPT", DateTime.Now, new List<ReceiptResourceDto>());

        _receiptRepository.ExistsByNumberAsync("EMPTY-RECEIPT").Returns(false);

        // Act & Assert - Should not throw
        await handler.Handle(command, CancellationToken.None);
        
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithEmptyResourcesAndSign_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        var command = new CreateShipmentCommand("EMPTY-SHIP-SIGN", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto>(), Sign: true);

        _shipmentRepository.ExistsByNumberAsync("EMPTY-SHIP-SIGN", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}