using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.DeleteShipment;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.Exceptions;
using MediatR;

namespace WarehouseManagement.Tests.Application.Features.ShipmentDocuments.Commands;

public class DeleteShipmentCommandHandlerTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteShipmentCommandHandler _handler;

    public DeleteShipmentCommandHandlerTests()
    {
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new DeleteShipmentCommandHandler(
            _shipmentRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_throw_exception_when_shipment_not_found()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((ShipmentDocument?)null);

        // Act
        var action = async () => await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"ShipmentDocument with ID {shipmentId} was not found");
    }

    [Fact]
    public async Task handle_should_throw_exception_when_trying_to_delete_signed_shipment()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act
        var action = async () => await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SignedDocumentException>();
    }

    [Fact]
    public async Task handle_should_delete_unsigned_shipment_successfully()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        existingShipment.AddResource(Guid.NewGuid(), Guid.NewGuid(), 100m);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _shipmentRepository.Received(1).Delete(existingShipment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_delete_empty_unsigned_shipment()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        // No resources added
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _shipmentRepository.Received(1).Delete(existingShipment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_delete_shipment_with_multiple_resources()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        existingShipment.AddResource(Guid.NewGuid(), Guid.NewGuid(), 50m);
        existingShipment.AddResource(Guid.NewGuid(), Guid.NewGuid(), 75m);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _shipmentRepository.Received(1).Delete(existingShipment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_not_call_save_changes_when_shipment_not_found()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((ShipmentDocument?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
        
        _shipmentRepository.DidNotReceive().Delete(Arg.Any<ShipmentDocument>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_not_call_save_changes_when_shipment_is_signed()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act & Assert
        var action = async () => await _handler.Handle(new DeleteShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<SignedDocumentException>();
        
        _shipmentRepository.DidNotReceive().Delete(Arg.Any<ShipmentDocument>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}