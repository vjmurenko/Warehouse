using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class RevokeShipmentDocumentTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBalanceService _balanceService;
    private readonly RevokeShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    
    public RevokeShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _balanceService = Substitute.For<IBalanceService>();
        
        // Initialize handler
        _handler = new RevokeShipmentCommandHandler(_shipmentRepository, _balanceService, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
    }

    [Fact]
    public async Task RevokeSignedShipmentDocument_ShouldRestoreBalanceAndRevoke()
    {
        // Arrange
        var signedDocument = new ShipmentDocument("SIGNED_DOC", _defaultClientId, DateTime.Now);
        signedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        signedDocument.Sign(); // Make it signed
        
        var command = new RevokeShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(signedDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).IncreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Is<Quantity>(q => q.Value == 20), Arg.Any<CancellationToken>());
        
        await _shipmentRepository.Received(1).UpdateAsync(
            Arg.Is<ShipmentDocument>(s => !s.IsSigned),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.False(signedDocument.IsSigned);
    }

    [Fact]
    public async Task RevokeSignedShipmentDocument_WithMultipleResources_ShouldRestoreAllBalances()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var signedDocument = new ShipmentDocument("MULTI_SIGNED", _defaultClientId, DateTime.Now);
        signedDocument.AddResource(resource1Id, unit1Id, 15);
        signedDocument.AddResource(resource2Id, unit2Id, 25);
        signedDocument.Sign();
        
        var command = new RevokeShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(signedDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _balanceService.Received(1).IncreaseBalance(resource1Id, unit1Id, 
            Arg.Is<Quantity>(q => q.Value == 15), Arg.Any<CancellationToken>());
        await _balanceService.Received(1).IncreaseBalance(resource2Id, unit2Id, 
            Arg.Is<Quantity>(q => q.Value == 25), Arg.Any<CancellationToken>());
        
        await _shipmentRepository.Received(1).UpdateAsync(
            Arg.Is<ShipmentDocument>(s => !s.IsSigned),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeUnsignedShipmentDocument_ShouldThrowException()
    {
        // Arrange
        var unsignedDocument = new ShipmentDocument("UNSIGNED_DOC", _defaultClientId, DateTime.Now);
        unsignedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        // Document is not signed
        
        var command = new RevokeShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(unsignedDocument);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Документ не подписан и не может быть отозван", exception.Message);
        
        await _balanceService.DidNotReceive().IncreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeNonExistentShipmentDocument_ShouldThrowException()
    {
        // Arrange
        var command = new RevokeShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns((ShipmentDocument?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с ID {_defaultDocumentId} не найден", exception.Message);
        
        await _balanceService.DidNotReceive().IncreaseBalance(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Quantity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeShipmentDocument_WhenBalanceServiceFails_ShouldRollbackTransaction()
    {
        // Arrange
        var signedDocument = new ShipmentDocument("SIGNED_FAIL", _defaultClientId, DateTime.Now);
        signedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        signedDocument.Sign();
        
        var command = new RevokeShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(signedDocument);
        _balanceService.IncreaseBalance(_defaultResourceId, _defaultUnitOfMeasureId, 
            Arg.Any<Quantity>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Balance service error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Balance service error", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}