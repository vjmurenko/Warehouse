using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Tests.Application.Features.ShipmentDocuments.Commands;

public class CreateShipmentCommandHandlerTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IBalanceService _balanceService;
    private readonly IShipmentValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateShipmentCommandHandler _handler;

    public CreateShipmentCommandHandlerTests()
    {
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _balanceService = Substitute.For<IBalanceService>();
        _validationService = Substitute.For<IShipmentValidationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        _handler = new CreateShipmentCommandHandler(
            _shipmentRepository,
            _balanceService,
            _validationService,
            _unitOfWork);
    }

    [Fact]
    public async Task handle_should_create_unsigned_shipment_when_sign_is_false()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateShipmentCommand(
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) },
            Sign: false
        );

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        await _validationService.Received(1).ValidateClient(clientId, null, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => resources.Count() == 1),
            Arg.Any<CancellationToken>());
        
        _shipmentRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ShipmentAggregate.ShipmentDocument>());
        
        // Should not decrease balance for unsigned shipment
        await _balanceService.DidNotReceive().DecreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_create_signed_shipment_and_decrease_balance_when_sign_is_true()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateShipmentCommand(
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) },
            Sign: true
        );

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        await _validationService.Received(1).ValidateClient(clientId, null, Arg.Any<CancellationToken>());
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => resources.Count() == 1),
            Arg.Any<CancellationToken>());
        
        _shipmentRepository.Received(1).Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ShipmentAggregate.ShipmentDocument>());
        
        // Should decrease balance for signed shipment
        await _balanceService.Received(1).DecreaseBalances(Arg.Any<IEnumerable<BalanceDelta>>(), Arg.Any<CancellationToken>());
        
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_exception_when_document_number_already_exists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var command = new CreateShipmentCommand("SHIP-001", clientId, DateTime.UtcNow, new List<ShipmentResourceDto>());

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Документ с номером SHIP-001 уже существует");
        
        _shipmentRepository.DidNotReceive().Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ShipmentAggregate.ShipmentDocument>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_validate_resources_have_sufficient_balance_when_signing()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateShipmentCommand(
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) },
            Sign: true
        );

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => 
                resources.Count() == 1 &&
                resources.First().ResourceId == resourceId &&
                resources.First().Quantity == 100m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_throw_when_validation_fails()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateShipmentCommand(
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> { new(resourceId, unitId, 100m) }
        );

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);
        
        _validationService.ValidateClient(clientId, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Client not found")));

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client not found");
        
        _shipmentRepository.DidNotReceive().Create(Arg.Any<WarehouseManagement.Domain.Aggregates.ShipmentAggregate.ShipmentDocument>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task handle_should_handle_multiple_resources()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        
        var command = new CreateShipmentCommand(
            "SHIP-001",
            clientId,
            DateTime.UtcNow,
            new List<ShipmentResourceDto> 
            { 
                new(resourceId1, unitId, 50m),
                new(resourceId2, unitId, 75m)
            },
            Sign: true
        );

        _shipmentRepository.ExistsByNumberAsync(command.Number, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        await _validationService.Received(1).ValidateShipmentResourcesForUpdate(
            Arg.Is<List<ShipmentResourceDto>>(resources => 
                resources.Count() == 2 &&
                resources.Any(r => r.ResourceId == resourceId1 && r.Quantity == 50m) &&
                resources.Any(r => r.ResourceId == resourceId2 && r.Quantity == 75m)),
            Arg.Any<CancellationToken>());
    }
}