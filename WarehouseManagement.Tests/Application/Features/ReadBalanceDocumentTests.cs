using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.Queries.GetBalances;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class ReadBalanceDocumentTests
{
    private readonly IBalanceRepository _balanceRepository;
    private readonly IResourceService _resourceService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;
    private readonly GetBalancesQueryHandler _getBalancesHandler;
    
    // Common test data
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly Balance _defaultBalance;
    
    public ReadBalanceDocumentTests()
    {
        // Initialize mocks
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _resourceService = Substitute.For<IResourceService>();
        _unitOfMeasureService = Substitute.For<IUnitOfMeasureService>();
        
        // Initialize handler
        _getBalancesHandler = new GetBalancesQueryHandler(_balanceRepository, _resourceService, _unitOfMeasureService);
        
        // Initialize common test data
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
        _defaultBalance = new Balance(_defaultResourceId, _defaultUnitOfMeasureId, new Quantity(25.5m));
        _defaultBalance.GetType().GetProperty("Id")?.SetValue(_defaultBalance, Guid.NewGuid());
    }

    [Fact]
    public async Task GetBalances_WithoutFilters_ShouldReturnAllBalances()
    {
        // Arrange
        var balance1 = new Balance(_defaultResourceId, _defaultUnitOfMeasureId, new Quantity(10.0m));
        var balance2 = new Balance(Guid.NewGuid(), Guid.NewGuid(), new Quantity(20.0m));
        balance1.GetType().GetProperty("Id")?.SetValue(balance1, Guid.NewGuid());
        balance2.GetType().GetProperty("Id")?.SetValue(balance2, Guid.NewGuid());

        var balances = new List<Balance> { balance1, balance2 };
        var query = new GetBalancesQuery();

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(balances);
        _resourceService.GetByIdAsync(Arg.Any<Guid>())
            .Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(Arg.Any<Guid>())
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => 
        {
            Assert.Equal("Test Resource", dto.ResourceName);
            Assert.Equal("Test Unit", dto.UnitOfMeasureName);
        });
    }

    [Fact]
    public async Task GetBalances_WithResourceFilter_ShouldPassCorrectParameters()
    {
        // Arrange
        var resourceIds = new List<Guid> { _defaultResourceId, Guid.NewGuid() };
        var query = new GetBalancesQuery(resourceIds);

        _balanceRepository.GetFilteredAsync(resourceIds, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance>());

        // Act
        await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        await _balanceRepository.Received(1).GetFilteredAsync(resourceIds, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBalances_WithUnitFilter_ShouldPassCorrectParameters()
    {
        // Arrange
        var unitIds = new List<Guid> { _defaultUnitOfMeasureId, Guid.NewGuid() };
        var query = new GetBalancesQuery(null, unitIds);

        _balanceRepository.GetFilteredAsync(null, unitIds, Arg.Any<CancellationToken>())
            .Returns(new List<Balance>());

        // Act
        await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        await _balanceRepository.Received(1).GetFilteredAsync(null, unitIds, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBalances_WithBothFilters_ShouldPassAllParameters()
    {
        // Arrange
        var resourceIds = new List<Guid> { _defaultResourceId };
        var unitIds = new List<Guid> { _defaultUnitOfMeasureId };
        var query = new GetBalancesQuery(resourceIds, unitIds);

        _balanceRepository.GetFilteredAsync(resourceIds, unitIds, Arg.Any<CancellationToken>())
            .Returns(new List<Balance> { _defaultBalance });
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        await _balanceRepository.Received(1).GetFilteredAsync(resourceIds, unitIds, Arg.Any<CancellationToken>());
        Assert.Single(result);
        Assert.Equal(_defaultResourceId, result.First().ResourceId);
        Assert.Equal(_defaultUnitOfMeasureId, result.First().UnitOfMeasureId);
    }

    [Fact]
    public async Task GetBalances_WithMissingResourceData_ShouldSkipMissingResources()
    {
        // Arrange
        var query = new GetBalancesQuery();

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance> { _defaultBalance });
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns((Resource?)null); // Missing resource
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result); // Should skip balances with missing resource data
    }

    [Fact]
    public async Task GetBalances_WithMissingUnitData_ShouldSkipMissingUnits()
    {
        // Arrange
        var query = new GetBalancesQuery();

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance> { _defaultBalance });
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns((UnitOfMeasure?)null); // Missing unit

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result); // Should skip balances with missing unit data
    }

    [Fact]
    public async Task GetBalances_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetBalancesQuery();

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance>());

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBalances_WithValidData_ShouldMapCorrectly()
    {
        // Arrange
        var query = new GetBalancesQuery();
        var balanceId = Guid.NewGuid();
        _defaultBalance.GetType().GetProperty("Id")?.SetValue(_defaultBalance, balanceId);

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance> { _defaultBalance });
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        var dto = result.First();
        Assert.Equal(balanceId, dto.Id);
        Assert.Equal(_defaultResourceId, dto.ResourceId);
        Assert.Equal("Test Resource", dto.ResourceName);
        Assert.Equal(_defaultUnitOfMeasureId, dto.UnitOfMeasureId);
        Assert.Equal("Test Unit", dto.UnitOfMeasureName);
        Assert.Equal(25.5m, dto.Quantity);
    }

    [Fact]
    public async Task GetBalances_WithMultipleResourcesAndUnits_ShouldMapAllCorrectly()
    {
        // Arrange
        var resource2Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        var resource2 = new Resource("Test Resource 2") { Id = resource2Id };
        var unit2 = new UnitOfMeasure("Test Unit 2") { Id = unit2Id };
        
        var balance1 = new Balance(_defaultResourceId, _defaultUnitOfMeasureId, new Quantity(10.0m));
        var balance2 = new Balance(resource2Id, unit2Id, new Quantity(20.0m));
        balance1.GetType().GetProperty("Id")?.SetValue(balance1, Guid.NewGuid());
        balance2.GetType().GetProperty("Id")?.SetValue(balance2, Guid.NewGuid());

        var query = new GetBalancesQuery();

        _balanceRepository.GetFilteredAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Balance> { balance1, balance2 });
        
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns(_defaultResource);
        _resourceService.GetByIdAsync(resource2Id)
            .Returns(resource2);
        
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);
        _unitOfMeasureService.GetByIdAsync(unit2Id)
            .Returns(unit2);

        // Act
        var result = await _getBalancesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        
        var firstBalance = result.First(r => r.ResourceId == _defaultResourceId);
        Assert.Equal("Test Resource", firstBalance.ResourceName);
        Assert.Equal("Test Unit", firstBalance.UnitOfMeasureName);
        Assert.Equal(10.0m, firstBalance.Quantity);
        
        var secondBalance = result.First(r => r.ResourceId == resource2Id);
        Assert.Equal("Test Resource 2", secondBalance.ResourceName);
        Assert.Equal("Test Unit 2", secondBalance.UnitOfMeasureName);
        Assert.Equal(20.0m, secondBalance.Quantity);
    }
}