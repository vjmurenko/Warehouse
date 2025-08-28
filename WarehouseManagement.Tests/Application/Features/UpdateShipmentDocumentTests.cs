using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class UpdateShipmentDocumentTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IReceiptValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly UpdateShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    
    public UpdateShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _validationService = Substitute.For<IReceiptValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new UpdateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
    }

    [Fact]
    public async Task UpdateUnsignedShipmentDocument_ShouldUpdateSuccessfully()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("OLD_NUMBER", _defaultClientId, DateTime.Now.AddDays(-1));
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
        
        var newResourceId = Guid.NewGuid();
        var newResource = new Resource("New Resource") { Id = newResourceId };
        var shipmentDto = new ShipmentResourceDto(newResourceId, _defaultUnitOfMeasureId, 25);
        var command = new UpdateShipmentCommand(_defaultDocumentId, "NEW_NUMBER", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("NEW_NUMBER", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(newResourceId, Arg.Any<CancellationToken>()).Returns(newResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).UpdateAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == "NEW_NUMBER" &&
                s.ShipmentResources.Count == 1 &&
                s.ShipmentResources.First().ResourceId == newResourceId &&
                s.ShipmentResources.First().Quantity.Value == 25 &&
                !s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _balanceService.DidNotReceive().IncreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSignedShipmentDocument_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("OLD_SIGNED", _defaultClientId, DateTime.Now.AddDays(-1));
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
        originalDocument.Sign(); // Make it signed
        
        var newResourceId = Guid.NewGuid();
        var newResource = new Resource("New Resource") { Id = newResourceId };
        var shipmentDto = new ShipmentResourceDto(newResourceId, _defaultUnitOfMeasureId, 25);
        var command = new UpdateShipmentCommand(_defaultDocumentId, "NEW_SIGNED", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Подписанный документ отгрузки нельзя редактировать", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSignedShipmentDocument_WithImmediateSigning_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("OLD_SIGNED", _defaultClientId, DateTime.Now.AddDays(-1));
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
        originalDocument.Sign(); // Make it signed
        
        var newResourceId = Guid.NewGuid();
        var newResource = new Resource("New Resource") { Id = newResourceId };
        var shipmentDto = new ShipmentResourceDto(newResourceId, _defaultUnitOfMeasureId, 25);
        var command = new UpdateShipmentCommand(_defaultDocumentId, "NEW_SIGNED_IMM", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto }, Sign: true);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Подписанный документ отгрузки нельзя редактировать", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithDuplicateNumber_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        var command = new UpdateShipmentCommand(_defaultDocumentId, "DUPLICATE", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("DUPLICATE", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ с номером DUPLICATE уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithNonExistentDocument_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateShipmentCommand(_defaultDocumentId, "TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns((ShipmentDocument?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с ID {_defaultDocumentId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        var invalidResourceId = Guid.NewGuid();
        var command = new UpdateShipmentCommand(_defaultDocumentId, "TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(invalidResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(invalidResourceId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {invalidResourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {invalidResourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithImmediateSigningAndInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var shipmentDto = new ShipmentResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, 1000); // Large quantity
        var command = new UpdateShipmentCommand(_defaultDocumentId, "TEST_BALANCE", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { shipmentDto }, Sign: true);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("TEST_BALANCE", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateResourceAsync(_defaultResourceId, Arg.Any<CancellationToken>()).Returns(_defaultResource);
        _validationService.ValidateUnitOfMeasureAsync(_defaultUnitOfMeasureId, Arg.Any<CancellationToken>()).Returns(_defaultUnitOfMeasure);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 1000), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурсов на складе"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурсов на складе", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithEmptyResources_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var command = new UpdateShipmentCommand(_defaultDocumentId, "EMPTY_TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto>());

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("EMPTY_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert - Должно падать по бизнес-правилу "документ не может быть пустым"
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ отгрузки не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithEmptyResourcesAndImmediateSigning_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var command = new UpdateShipmentCommand(_defaultDocumentId, "EMPTY_SIGN_TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto>(), Sign: true);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("EMPTY_SIGN_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert - Должно падать ещё до попытки подписания
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ отгрузки не может быть пустым", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}