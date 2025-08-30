using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Tests.Application.Features;

public class DeleteShipmentDocumentTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteShipmentCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    
    public DeleteShipmentDocumentTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        // Initialize handler
        _handler = new DeleteShipmentCommandHandler(_shipmentRepository, _unitOfWork);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
    }

    [Fact]
    public async Task DeleteUnsignedShipmentDocument_ShouldDeleteSuccessfully()
    {
        // Arrange
        var unsignedDocument = new ShipmentDocument("UNSIGNED_TO_DELETE", _defaultClientId, DateTime.Now);
        unsignedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        // Document is not signed
        
        var command = new DeleteShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(unsignedDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).DeleteAsync(unsignedDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteSignedShipmentDocument_ShouldThrowException()
    {
        // Arrange
        var signedDocument = new ShipmentDocument("SIGNED_PROTECTED", _defaultClientId, DateTime.Now);
        signedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        signedDocument.Sign(); // Make it signed
        
        var command = new DeleteShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(signedDocument);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SignedDocumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Cannot delete signed shipment document", exception.Message);
        
        await _shipmentRepository.DidNotReceive().DeleteAsync(Arg.Any<ShipmentDocument>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteNonExistentShipmentDocument_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns((ShipmentDocument?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"ShipmentDocument with ID {_defaultDocumentId} was not found", exception.Message);
        
        await _shipmentRepository.DidNotReceive().DeleteAsync(Arg.Any<ShipmentDocument>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUnsignedShipmentDocument_WithMultipleResources_ShouldDeleteSuccessfully()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var unsignedDocument = new ShipmentDocument("MULTI_UNSIGNED", _defaultClientId, DateTime.Now);
        unsignedDocument.AddResource(resource1Id, unit1Id, 15);
        unsignedDocument.AddResource(resource2Id, unit2Id, 25);
        // Document is not signed
        
        var command = new DeleteShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(unsignedDocument);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).DeleteAsync(unsignedDocument, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteShipmentDocument_WhenRepositoryFails_ShouldRollbackTransaction()
    {
        // Arrange
        var unsignedDocument = new ShipmentDocument("FAIL_DELETE", _defaultClientId, DateTime.Now);
        unsignedDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        
        var command = new DeleteShipmentCommand(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(unsignedDocument);
        _shipmentRepository.DeleteAsync(unsignedDocument, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Repository error", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}