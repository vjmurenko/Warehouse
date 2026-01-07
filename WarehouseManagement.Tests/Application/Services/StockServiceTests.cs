using FluentAssertions;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Tests.Application.Services;

public class StockServiceTests
{
    private readonly IStockMovementRepository _movementRepository;
    private readonly INamedEntityRepository<Resource> _resourceRepository;
    private readonly INamedEntityRepository<UnitOfMeasure> _unitRepository;
    private readonly StockService _stockService;

    public StockServiceTests()
    {
        _movementRepository = Substitute.For<IStockMovementRepository>();
        _resourceRepository = Substitute.For<INamedEntityRepository<Resource>>();
        _unitRepository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _stockService = new StockService(_movementRepository, _resourceRepository, _unitRepository);
    }

    [Fact]
    public async Task RecordMovements_should_add_movements_to_repository()
    {
        var documentId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var items = new List<(Guid, Guid, decimal)> { (resourceId, unitId, 100m) };

        await _stockService.RecordMovements(documentId, MovementType.Receipt, items, CancellationToken.None);

        await _movementRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<StockMovement>>(m => 
                m.Count() == 1 && 
                m.First().DocumentId == documentId &&
                m.First().ResourceId == resourceId &&
                m.First().Quantity == 100m &&
                m.First().Type == MovementType.Receipt),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordMovements_should_skip_zero_quantity_items()
    {
        var documentId = Guid.NewGuid();
        var items = new List<(Guid, Guid, decimal)> { (Guid.NewGuid(), Guid.NewGuid(), 0m) };

        await _stockService.RecordMovements(documentId, MovementType.Receipt, items, CancellationToken.None);

        await _movementRepository.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<StockMovement>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReverseMovements_should_create_reversal_movements()
    {
        var documentId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var existingMovement = StockMovement.Create(resourceId, unitId, 100m, documentId, MovementType.Receipt);

        _movementRepository.GetByDocumentIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(new List<StockMovement> { existingMovement });

        await _stockService.ReverseMovements(documentId, CancellationToken.None);

        await _movementRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<StockMovement>>(m => 
                m.Count() == 1 && 
                m.First().Quantity == -100m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAvailability_should_not_throw_when_sufficient_balance()
    {
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var items = new List<(Guid, Guid, decimal)> { (resourceId, unitId, 50m) };

        _movementRepository.GetBalancesAsync(Arg.Any<IEnumerable<(Guid, Guid)>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<(Guid, Guid), decimal> { { (resourceId, unitId), 100m } });

        var action = async () => await _stockService.ValidateAvailability(items, CancellationToken.None);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAvailability_should_throw_when_insufficient_balance()
    {
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var items = new List<(Guid, Guid, decimal)> { (resourceId, unitId, 150m) };

        _movementRepository.GetBalancesAsync(Arg.Any<IEnumerable<(Guid, Guid)>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<(Guid, Guid), decimal> { { (resourceId, unitId), 100m } });

        _resourceRepository.GetByIdAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns(Resource.Create("Test Resource"));
        _unitRepository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(UnitOfMeasure.Create("kg"));

        var action = async () => await _stockService.ValidateAvailability(items, CancellationToken.None);

        await action.Should().ThrowAsync<InsufficientBalanceException>()
            .WithMessage("*Test Resource*kg*150*100*");
    }

    [Fact]
    public async Task ValidateAvailability_should_throw_when_no_balance_exists()
    {
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var items = new List<(Guid, Guid, decimal)> { (resourceId, unitId, 50m) };

        _movementRepository.GetBalancesAsync(Arg.Any<IEnumerable<(Guid, Guid)>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<(Guid, Guid), decimal>());

        _resourceRepository.GetByIdAsync(resourceId, Arg.Any<CancellationToken>())
            .Returns(Resource.Create("Test Resource"));
        _unitRepository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(UnitOfMeasure.Create("kg"));

        var action = async () => await _stockService.ValidateAvailability(items, CancellationToken.None);

        await action.Should().ThrowAsync<InsufficientBalanceException>();
    }

    [Fact]
    public async Task ValidateAvailability_should_skip_zero_required_items()
    {
        var items = new List<(Guid, Guid, decimal)> { (Guid.NewGuid(), Guid.NewGuid(), 0m) };

        await _stockService.ValidateAvailability(items, CancellationToken.None);

        await _movementRepository.DidNotReceive().GetBalancesAsync(
            Arg.Any<IEnumerable<(Guid, Guid)>>(),
            Arg.Any<CancellationToken>());
    }
}
