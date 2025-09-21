using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.RevokeShipment;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.Exceptions;
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
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
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
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        existingShipment.AddResource(resourceId, unitId, 100m);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        // Should restore balance by increasing it
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 1 &&
                deltas.First().ResourceId == resourceId &&
                deltas.First().Quantity == 100m),
            Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        existingShipment.AddResource(resourceId1, unitId, 50m);
        existingShipment.AddResource(resourceId2, unitId, 75m);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        // Should restore balance for all resources
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 2 &&
                deltas.Any(d => d.ResourceId == resourceId1 && d.Quantity == 50m) &&
                deltas.Any(d => d.ResourceId == resourceId2 && d.Quantity == 75m)),
            Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_revoke_shipment_with_same_resource_different_units()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId1 = Guid.NewGuid();
        var unitId2 = Guid.NewGuid();
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        existingShipment.AddResource(resourceId, unitId1, 50m);
        existingShipment.AddResource(resourceId, unitId2, 75m);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        // Should restore balance for both unit combinations
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 2 &&
                deltas.Any(d => d.ResourceId == resourceId && d.UnitOfMeasureId == unitId1 && d.Quantity == 50m) &&
                deltas.Any(d => d.ResourceId == resourceId && d.UnitOfMeasureId == unitId2 && d.Quantity == 75m)),
            Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_handle_shipment_with_duplicate_resource_unit_combinations()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        existingShipment.AddResource(resourceId, unitId, 25m);
        existingShipment.AddResource(resourceId, unitId, 75m); // Same resource and unit
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        // Should create separate deltas for each resource (not grouped)
        await _balanceService.Received(1).IncreaseBalances(
            Arg.Is<IEnumerable<BalanceDelta>>(deltas => 
                deltas.Count() == 2 &&
                deltas.Any(d => d.ResourceId == resourceId && d.UnitOfMeasureId == unitId && d.Quantity == 25m) &&
                deltas.Any(d => d.ResourceId == resourceId && d.UnitOfMeasureId == unitId && d.Quantity == 75m)),
            Arg.Any<CancellationToken>());
        
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
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        await _balanceService.DidNotReceive().IncreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_not_call_save_changes_when_shipment_is_not_signed()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act & Assert
        var action = async () => await _handler.Handle(new RevokeShipmentCommand(shipmentId), CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>();
        
        await _balanceService.DidNotReceive().IncreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}