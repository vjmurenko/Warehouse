using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using MediatR;

namespace WarehouseManagement.Tests.Application.Features.ShipmentDocuments.Commands;

public class RevokeShipmentCommandHandlerTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RevokeShipmentCommandHandler _handler;

    public RevokeShipmentCommandHandlerTests()
    {
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new RevokeShipmentCommandHandler(
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
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Документ с ID {shipmentId} не найден");
    }

    [Fact]
    public async Task handle_should_throw_exception_when_shipment_is_not_signed()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = ShipmentDocument.Create("SHIP-001", clientId, DateTime.UtcNow, []);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ не подписан и не может быть отозван");
    }

    [Fact]
    public async Task handle_should_revoke_signed_shipment_and_restore_balance()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var resource = ShipmentResource.Create(shipmentId, resourceId, unitId, 100m);
        var existingShipment = ShipmentDocument.Create("SHIP-001", clientId, DateTime.UtcNow, [resource]);
        existingShipment.Sign(); // Sign the document first
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_revoke_shipment_with_multiple_resources()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var resource1 = ShipmentResource.Create(shipmentId, resourceId1, unitId, 50m);
        var resource2 = ShipmentResource.Create(shipmentId, resourceId2, unitId, 75m);
        var existingShipment = ShipmentDocument.Create("SHIP-001", clientId, DateTime.UtcNow, [resource1, resource2]);
        existingShipment.Sign(); // Sign the document first
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_not_call_save_changes_when_shipment_not_found()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((ShipmentDocument?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        await _unitOfWork.DidNotReceive().SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_not_call_save_changes_when_shipment_is_not_signed()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = ShipmentDocument.Create("SHIP-001", clientId, DateTime.UtcNow, []);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act & Assert
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        await _unitOfWork.DidNotReceive().SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }
}
