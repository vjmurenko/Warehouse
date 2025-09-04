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
    private readonly IShipmentValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly CreateShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly Client _defaultClient;
    
    public CreateShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _validationService = Substitute.For<IShipmentValidationService>();
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
        _defaultClient = new Client("Test Client", "Test Address") { Id = _defaultClientId };
    }

    [Fact]
    public async Task CreateShipmentDocument_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var quantity = 20m;
        var documentNumber = "SH-123";
        var date = DateTime.Now;

        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, date, new List<ShipmentResourceDto> { shipmentDto });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == documentNumber &&
                s.ClientId == _defaultClientId &&
                s.ShipmentResources.Count == 1 &&
                s.ShipmentResources.First().Quantity.Value == quantity &&
                s.ShipmentResources.First().ResourceId == _defaultResourceId &&
                s.ShipmentResources.First().UnitOfMeasureId == _defaultUnitOfMeasureId &&
                !s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _validationService.Received(1).ValidateClient(_defaultClientId);
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>());
        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocument_WithSigning_ShouldCreateAndSignSuccessfully()
    {
        // Arrange
        var quantity = 15m;
        var documentNumber = "SH-SIGN-123";
        var date = DateTime.Now;

        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity);
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, date, new List<ShipmentResourceDto> { shipmentDto }, Sign: true);

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == documentNumber &&
                s.ClientId == _defaultClientId &&
                s.ShipmentResources.Count == 1 &&
                s.ShipmentResources.First().Quantity.Value == quantity &&
                s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocument_WithDuplicateNumber_ShouldThrowException()
    {
        // Arrange
        var documentNumber = "DUPLICATE_SH_123";
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
    public async Task CreateShipmentDocument_WithArchivedClient_ShouldThrowException()
    {
        // Arrange
        var archivedClientId = Guid.NewGuid();
        var documentNumber = "SH_ARCHIVED_CLIENT";
        var command = new CreateShipmentCommand(documentNumber, archivedClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(archivedClientId)
            .ThrowsAsync(new InvalidOperationException("Клиент TestArchived находится в архиве и не может быть использован"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("находится в архиве и не может быть использован", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var invalidResourceId = Guid.NewGuid();
        var documentNumber = "SH_INVALID_RESOURCE";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(invalidResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {invalidResourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {invalidResourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithArchivedUnit_ShouldThrowException()
    {
        // Arrange
        var archivedUnitId = Guid.NewGuid();
        var documentNumber = "SH_ARCHIVED_UNIT";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, archivedUnitId, 10) });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Единица измерения 'кг' архивирована и не может быть использована"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирована и не может быть использована", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithEmptyResources_ShouldThrowException()
    {
        // Arrange
        var documentNumber = "SH_EMPTY";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, new List<ShipmentResourceDto>());

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ отгрузки не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var quantity = 1000m; // Large quantity
        var documentNumber = "SH_INSUFFICIENT_BALANCE";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, quantity) });

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурсов на складе"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурсов на складе", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShipmentDocument_WithSigningAndInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var quantity = 500m;
        var documentNumber = "SH_SIGN_INSUFFICIENT";
        var command = new CreateShipmentCommand(documentNumber, _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, quantity) }, Sign: true);

        _shipmentRepository.ExistsByNumberAsync(documentNumber, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == quantity), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурсов на складе для списания"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурсов на складе для списания", exception.Message);
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
        
        var command = new CreateShipmentCommand("MULTI_SH_123", _defaultClientId, DateTime.Now, shipments);

        _shipmentRepository.ExistsByNumberAsync("MULTI_SH_123", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(resource1Id, unit1Id, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(resource2Id, unit2Id, 
            Arg.Is<Quantity>(q => q.Value == 20), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s => s.Number == "MULTI_SH_123" && s.ShipmentResources.Count == 2),
            Arg.Any<CancellationToken>());
        
        await _balanceService.Received(1).ValidateBalanceAvailability(resource1Id, unit1Id, 
            Arg.Is<Quantity>(q => q.Value == 10), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).ValidateBalanceAvailability(resource2Id, unit2Id, 
            Arg.Is<Quantity>(q => q.Value == 20), Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateShipmentDocument_WithMultipleResourcesAndSigning_ShouldCreateAndSignCorrectly()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        var quantity1 = new Quantity(10);
        var quantity2 = new Quantity(20);
        
        var shipments = new List<ShipmentResourceDto>
        {
            new(resource1Id, unit1Id, quantity1.Value),
            new(resource2Id, unit2Id, quantity2.Value)
        };
        
        var command = new CreateShipmentCommand("MULTI_SIGN_SH_123", _defaultClientId, DateTime.Now, shipments, Sign: true);

        _shipmentRepository.ExistsByNumberAsync("MULTI_SIGN_SH_123", cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(resource1Id, unit1Id, quantity1, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(resource2Id, unit2Id, quantity2, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.DecreaseBalance(resource1Id, unit1Id, quantity1, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.DecreaseBalance(resource2Id, unit2Id, quantity2, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).AddAsync(
            Arg.Is<ShipmentDocument>(s => s.Number == "MULTI_SIGN_SH_123" && s.ShipmentResources.Count == 2 && s.IsSigned),
            Arg.Any<CancellationToken>());
        
        await _balanceService.Received(1).DecreaseBalance(resource1Id, unit1Id, quantity1, Arg.Any<CancellationToken>());
        await _balanceService.Received(1).DecreaseBalance(resource2Id, unit2Id, quantity2, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }
}