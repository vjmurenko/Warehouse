using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using MediatR;

namespace WarehouseManagement.Tests.Application.Features.ShipmentDocuments.Commands;

public class UpdateShipmentCommandHandlerTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateShipmentCommandHandler _handler;

    public UpdateShipmentCommandHandlerTests()
    {
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _validationService = Substitute.For<IShipmentValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new UpdateShipmentCommandHandler(
            _shipmentRepository,
            _validationService,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_throw_exception_when_shipment_not_found()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto>());

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns((ShipmentDocument?)null);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Документ с ID {shipmentId} не найден");
    }

    [Fact]
    public async Task handle_should_throw_exception_when_trying_to_update_signed_shipment()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: true);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto>());

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Подписанный документ отгрузки нельзя редактировать. Используйте команду отзыва документа.");
    }

    [Fact]
    public async Task handle_should_throw_exception_when_number_already_exists()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-002", // Different number
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto>());

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(true); // Number exists for another document

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ с номером SHIP-002 уже существует");
    }

    [Fact]
    public async Task handle_should_update_unsigned_shipment_without_affecting_balance()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) },
            Sign: false);

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        await _validationService.Received(1).ValidateClient(clientId, clientId, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => resources.Count() == 1),
            Arg.Any<CancellationToken>(),
            existingShipment);
        
        // Should not affect balance for unsigned shipment
        await _balanceService.DidNotReceive().DecreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_update_and_sign_shipment_decreasing_balance()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) },
            Sign: true);

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        await _validationService.Received(1).ValidateClient(clientId, clientId, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => resources.Count() == 1),
            Arg.Any<CancellationToken>(),
            existingShipment);
        
        // Should decrease balance when signing
        await _balanceService.Received(1).DecreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_validate_client_and_resources()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> 
            { 
                new(resourceId1, unitId, 50m),
                new(resourceId2, unitId, 75m)
            });

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _validationService.Received(1).ValidateClient(clientId, clientId, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => 
                resources.Count() == 2 &&
                resources.Any(r => r.ResourceId == resourceId1 && r.Quantity == 50m) &&
                resources.Any(r => r.ResourceId == resourceId2 && r.Quantity == 75m)),
            Arg.Any<CancellationToken>(),
            existingShipment);
    }

    [Fact]
    public async Task handle_should_throw_when_updating_to_empty_resources_list()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingShipment = new ShipmentDocument("SHIP-001", clientId, DateTime.UtcNow, isSigned: false);
        existingShipment.AddResource(Guid.NewGuid(), Guid.NewGuid(), 100m);
        
        var command = new UpdateShipmentCommand(
            shipmentId,
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto>(), // Empty resources
            Sign: false);

        _shipmentRepository.GetByIdWithResourcesAsync(shipmentId, Arg.Any<CancellationToken>())
            .Returns(existingShipment);
        _shipmentRepository.ExistsByNumberAsync(command.Number, command.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ отгрузки не может быть пустым");
    }
}