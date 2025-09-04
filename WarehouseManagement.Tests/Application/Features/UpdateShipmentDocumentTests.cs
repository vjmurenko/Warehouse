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
    private readonly IShipmentValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly UpdateShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    
    public UpdateShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _validationService = Substitute.For<IShipmentValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new UpdateShipmentCommandHandler(_shipmentRepository, _balanceService, _validationService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
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
        _validationService.ValidateClient(command.ClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, CancellationToken.None, originalDocument).Returns(Task.CompletedTask);

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
        _validationService.ValidateClient(command.ClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, CancellationToken.None, originalDocument).Returns(Task.CompletedTask);

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

    [Fact]
    public async Task UpdateShipmentDocument_WithArchivedClient_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var archivedClientId = Guid.NewGuid();
        var command = new UpdateShipmentCommand(_defaultDocumentId, "ARCHIVED_CLIENT_TEST", archivedClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("ARCHIVED_CLIENT_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(archivedClientId, _defaultClientId)
            .ThrowsAsync(new InvalidOperationException("Клиент ArchivedClient находится в архиве и не может быть использован"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("находится в архиве и не может быть использован", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var invalidResourceId = Guid.NewGuid();
        var command = new UpdateShipmentCommand(_defaultDocumentId, "INVALID_RESOURCE_TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(invalidResourceId, _defaultUnitOfMeasureId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("INVALID_RESOURCE_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId, _defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>(), originalDocument)
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {invalidResourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {invalidResourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithArchivedUnit_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var archivedUnitId = Guid.NewGuid();
        var command = new UpdateShipmentCommand(_defaultDocumentId, "ARCHIVED_UNIT_TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, archivedUnitId, 10) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("ARCHIVED_UNIT_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId, _defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>(), originalDocument)
            .ThrowsAsync(new InvalidOperationException("Единица измерения 'кг' архивирована и не может быть использована"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирована и не может быть использована", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithBalanceValidationFailure_ShouldThrowException()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var largeQuantity = 1000m;
        var command = new UpdateShipmentCommand(_defaultDocumentId, "BALANCE_TEST", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, largeQuantity) });

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("BALANCE_TEST", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId, _defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>(), originalDocument)
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == largeQuantity), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Недостаточно ресурсов на складе"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Недостаточно ресурсов на складе", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithValidDataAndSigning_ShouldUpdateAndSignSuccessfully()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now.AddDays(-1));
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var newQuantity = 25m;
        var command = new UpdateShipmentCommand(_defaultDocumentId, "UPDATED_SIGNED", _defaultClientId, DateTime.Now, 
            new List<ShipmentResourceDto> { new(_defaultResourceId, _defaultUnitOfMeasureId, newQuantity) }, Sign: true);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("UPDATED_SIGNED", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId, _defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(command.Resources, Arg.Any<CancellationToken>(), originalDocument)
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == newQuantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == newQuantity), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).UpdateAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == "UPDATED_SIGNED" &&
                s.ShipmentResources.Count == 1 &&
                s.ShipmentResources.First().Quantity.Value == newQuantity &&
                s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _balanceService.Received(1).ValidateBalanceAvailability(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == newQuantity), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).DecreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == newQuantity), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateShipmentDocument_WithMultipleResourcesUpdates_ShouldHandleCorrectly()
    {
        // Arrange
        var originalDocument = new ShipmentDocument("ORIGINAL", _defaultClientId, DateTime.Now);
        originalDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 5);
        
        var newResource1Id = Guid.NewGuid();
        var newResource2Id = Guid.NewGuid();
        var newUnit1Id = Guid.NewGuid();
        var newUnit2Id = Guid.NewGuid();
        
        var newResources = new List<ShipmentResourceDto>
        {
            new(newResource1Id, newUnit1Id, 15),
            new(newResource2Id, newUnit2Id, 25)
        };
        
        var command = new UpdateShipmentCommand(_defaultDocumentId, "MULTI_UPDATED", _defaultClientId, DateTime.Now, newResources);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(originalDocument);
        _shipmentRepository.ExistsByNumberAsync("MULTI_UPDATED", _defaultDocumentId, Arg.Any<CancellationToken>()).Returns(false);
        _validationService.ValidateClient(_defaultClientId, _defaultClientId).Returns(Task.CompletedTask);
        _validationService.ValidateShipmentResourcesForUpdate(newResources, Arg.Any<CancellationToken>(), originalDocument)
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(newResource1Id, newUnit1Id, 
            Arg.Is<Quantity>(q => q.Value == 15), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _balanceService.ValidateBalanceAvailability(newResource2Id, newUnit2Id, 
            Arg.Is<Quantity>(q => q.Value == 25), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).UpdateAsync(
            Arg.Is<ShipmentDocument>(s =>
                s.Number == "MULTI_UPDATED" &&
                s.ShipmentResources.Count == 2 &&
                !s.IsSigned
            ),
            Arg.Any<CancellationToken>());

        await _balanceService.Received(1).ValidateBalanceAvailability(newResource1Id, newUnit1Id, 
            Arg.Is<Quantity>(q => q.Value == 15), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).ValidateBalanceAvailability(newResource2Id, newUnit2Id, 
            Arg.Is<Quantity>(q => q.Value == 25), Arg.Any<CancellationToken>());
        await _balanceService.DidNotReceive().DecreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}