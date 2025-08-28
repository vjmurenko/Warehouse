using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class CreateShipmentDocumentTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IReceiptValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly CreateShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    
    public CreateShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _validationService = Substitute.For<IReceiptValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new CreateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        
        // Initialize common test data
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
    }

    [Fact]
    public async Task CreateShipmentDocumentWithValidData()
    {
        // Arrange
        var quantity = new Quantity(20);
        var documentNumber = "SHIP_123";
        var date = DateTime.Now;

        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, date, 
            new List<ShipmentResourceDto> { shipmentDto });

        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);
        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == documentNumber &&
                s.ClientId == _defaultClientId &&
                s.ShipmentResources.Count == 1 &&
                s.ShipmentResources.First().Quantity.Value == quantity.Value &&
                s.ShipmentResources.First().ResourceId == _defaultResourceId &&
                s.ShipmentResources.First().UnitOfMeasureId == _defaultUnitOfMeasureId &&
                !s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _validationService.Received(1).ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>());
        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocumentWithImmediateSigning()
    {
        // Arrange
        var quantity = new Quantity(15);
        var documentNumber = "SHIP_456";
        var date = DateTime.Now;

        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, date, 
            new List<ShipmentResourceDto> { shipmentDto }, Sign: true);

        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);
        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == documentNumber &&
                s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocument_WithDuplicateNumber_ShouldThrowException()
    {
        // Arrange
        var documentNumber = "DUPLICATE_SHIP_123";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с номером {documentNumber} уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var shipmentDto = new ShipmentResourceDto(resourceId, unitId, 10);
        var command = new CreateShipmentCommand("SHIP_TEST_123", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto });

        _shipmentRepository.ExistsByNumberAsync("SHIP_TEST_123", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(resourceId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {resourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {resourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithArchivedUnit_ShouldThrowException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var shipmentDto = new ShipmentResourceDto(resourceId, unitId, 10);
        var command = new CreateShipmentCommand("SHIP_TEST_123", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto });
        var resource = new Resource("Test Resource") { Id = resourceId };

        _shipmentRepository.ExistsByNumberAsync("SHIP_TEST_123", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(resourceId, Arg.Any<CancellationToken>()).Returns(resource);
        _validationService.ValidateUnitOfMeasureAsync(unitId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Единица измерения 'кг' архивирована и не может быть использована"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирована и не может быть использована", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithMultipleResources_ShouldCreateCorrectly()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var shipments = new List<ShipmentResourceDto>
        {
            new(resource1Id, unit1Id, 10),
            new(resource2Id, unit2Id, 20)
        };
        
        var command = new CreateShipmentCommand("SHIP_MULTI_123", _defaultClientId, DateTime.Now, shipments);

        var resource1 = new Resource("Resource1") { Id = resource1Id };
        var resource2 = new Resource("Resource2") { Id = resource2Id };
        var unit1 = new UnitOfMeasure("Unit1") { Id = unit1Id };
        var unit2 = new UnitOfMeasure("Unit2") { Id = unit2Id };
        var quantity1 = new Quantity(10);
        var quantity2 = new Quantity(20);

        _shipmentRepository.ExistsByNumberAsync("SHIP_MULTI_123", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(resource1Id, Arg.Any<CancellationToken>()).Returns(resource1);
        _validationService.ValidateResourceAsync(resource2Id, Arg.Any<CancellationToken>()).Returns(resource2);
        _validationService.ValidateUnitOfMeasureAsync(unit1Id, Arg.Any<CancellationToken>()).Returns(unit1);
        _validationService.ValidateUnitOfMeasureAsync(unit2Id, Arg.Any<CancellationToken>()).Returns(unit2);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s => s.Number == "SHIP_MULTI_123" && s.ShipmentResources.Count == 2),
            Arg.Any<CancellationToken>());
        
        // Verify balance validation was called for both resources
        await _balanceService.Received(1).ValidateBalanceAvailability(resource1Id, unit1Id, quantity1, Arg.Any<CancellationToken>());
        await _balanceService.Received(1).ValidateBalanceAvailability(resource2Id, unit2Id, quantity2, Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocument_WithImmediateSigningAndInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var quantity = new Quantity(100);
        var documentNumber = "SHIP_BALANCE_TEST";
        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto }, Sign: true);

        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);
        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурсов на складе"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурсов на складе", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        
        // Verify validation was called but decrease was not called due to validation failure
        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithEmptyResources_ShouldThrowException()
    {
        // Arrange
        var command = new CreateShipmentCommand("SHIP_EMPTY", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto>());

        _shipmentRepository.ExistsByNumberAsync("SHIP_EMPTY", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert - Должно падать по бизнес-правилу "документ не может быть пустым"
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ отгрузки не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var quantity = new Quantity(100);
        var documentNumber = "SHIP_INSUFFICIENT_BALANCE";
        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto });

        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);
        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурса на складе. Доступно: 50, требуется: 100"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурса на складе", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        
        // Verify that balance was not decreased (only validation was called)
        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, quantity, Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithEmptyResourcesAndImmediateSigning_ShouldThrowException()
    {
        // Arrange
        var command = new CreateShipmentCommand("SHIP_EMPTY_SIGN", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto>(), Sign: true);

        _shipmentRepository.ExistsByNumberAsync("SHIP_EMPTY_SIGN", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert - Должно падать ещё до попытки подписания
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ отгрузки не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}