using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Dtos;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.ValueObjects;


namespace WarehouseManagement.Tests.Application.Services;

public class BalanceServiceTests
{
    private readonly IBalanceRepository _balanceRepository;
    private readonly INamedEntityRepository<Resource> _resourceRepository;
    private readonly INamedEntityRepository<UnitOfMeasure> _unitOfMeasureRepository;
    private readonly ILogger<BalanceService> _logger;
    private readonly BalanceService _balanceService;

    public BalanceServiceTests()
    {
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _resourceRepository = Substitute.For<INamedEntityRepository<Resource>>();
        _unitOfMeasureRepository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _logger = Substitute.For<ILogger<BalanceService>>();
        
        _balanceService = new BalanceService(
            _balanceRepository,
            _resourceRepository,
            _unitOfMeasureRepository,
            _logger);
    }

    [Fact]
    public async Task increase_balances_should_increase_existing_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(100m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        await _balanceService.IncreaseBalances(deltas, CancellationToken.None);

        // Assert
        existingBalance.Quantity.Value.Should().Be(150m);
    }

    [Fact]
    public async Task increase_balances_should_create_new_balance_when_none_exists()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var balances = new Dictionary<ResourceUnitKey, Balance>();
        
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        await _balanceService.IncreaseBalances(deltas, CancellationToken.None);

        // Assert
        await _balanceRepository.Received(1).AddAsync(
            Arg.Is<Balance>(b => b.ResourceId == resourceId && 
                               b.UnitOfMeasureId == unitId && 
                               b.Quantity.Value == 50m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task decrease_balances_should_decrease_existing_balance_when_sufficient_quantity_available()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(100m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 30m)
        };

        // Act
        await _balanceService.DecreaseBalances(deltas, CancellationToken.None);

        // Assert
        existingBalance.Quantity.Value.Should().Be(70m);
    }

    [Fact]
    public async Task decrease_balances_should_throw_exception_when_insufficient_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(30m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var resource = new Resource("Test Resource");
        var unit = new UnitOfMeasure("kg");
        
        _resourceRepository.GetByIdAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns(resource);
        _unitOfMeasureRepository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        var action = async () => await _balanceService.DecreaseBalances(deltas, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("Insufficient balance for Test Resource (kg). Requested: 50, Available: 30");
    }

    [Fact]
    public async Task decrease_balances_should_throw_exception_when_balance_does_not_exist()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var balances = new Dictionary<ResourceUnitKey, Balance>();
        
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var resource = new Resource("Test Resource");
        var unit = new UnitOfMeasure("kg");
        
        _resourceRepository.GetByIdAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns(resource);
        _unitOfMeasureRepository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        var action = async () => await _balanceService.DecreaseBalances(deltas, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("Insufficient balance for Test Resource (kg). Requested: 50, Available: 0");
    }

    [Fact]
    public async Task validate_balance_availability_should_not_throw_when_sufficient_balance_exists()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(100m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        var action = async () => await _balanceService.ValidateBalanceAvailability(deltas, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task validate_balance_availability_should_throw_when_insufficient_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(30m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var resource = new Resource("Test Resource");
        var unit = new UnitOfMeasure("kg");
        
        _resourceRepository.GetByIdAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns(resource);
        _unitOfMeasureRepository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 50m)
        };

        // Act
        var action = async () => await _balanceService.ValidateBalanceAvailability(deltas, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InsufficientBalanceException>();
    }

    [Fact]
    public async Task adjust_balances_should_handle_empty_deltas()
    {
        // Arrange
        var deltas = new List<BalanceDelta>();

        // Act
        var action = async () => await _balanceService.AdjustBalances(deltas, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        await _balanceRepository.DidNotReceive().GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task adjust_balances_should_skip_zero_quantity_deltas()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var key = new ResourceUnitKey(resourceId, unitId);
        var existingBalance = new Balance(resourceId, unitId, new Quantity(100m));

        var balances = new Dictionary<ResourceUnitKey, Balance> { { key, existingBalance } };
        _balanceRepository.GetForUpdateAsync(Arg.Any<IEnumerable<ResourceUnitKey>>(), Arg.Any<CancellationToken>())
            .Returns(balances);

        var deltas = new List<BalanceDelta>
        {
            new(resourceId, unitId, 0m)
        };

        var originalQuantity = existingBalance.Quantity.Value;

        // Act
        await _balanceService.AdjustBalances(deltas, CancellationToken.None);

        // Assert
        existingBalance.Quantity.Value.Should().Be(originalQuantity);
    }
}