using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Web;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class BalanceControllerTests : BaseIntegrationTest
{
    public BalanceControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBalances_ShouldReturnEmptyList_WhenNoBalances()
    {
        var balances = await GetAsync<List<BalanceDto>>("/api/Balance");

        Assert.NotNull(balances);
        Assert.Empty(balances);
    }

    [Fact]
    public async Task GetBalances_ShouldReturnBalances_WhenBalancesExist()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        
        var movement = StockMovement.Create(resource.Id, unit.Id, 100, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        var balances = await GetAsync<List<BalanceDto>>("/api/Balance");

        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(resource.Id, balances[0].ResourceId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithResourceFilter_ShouldReturnFilteredResults()
    {
        var resource1 = await CreateTestResourceAsync("Resource 1");
        var resource2 = await CreateTestResourceAsync("Resource 2");
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        
        var movement1 = StockMovement.Create(resource1.Id, unit.Id, 100, Guid.NewGuid(), MovementType.Receipt);
        var movement2 = StockMovement.Create(resource2.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.AddRange(movement1, movement2);
        await _context.SaveChangesAsync();

        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?resourceIds={resource1.Id}");

        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(resource1.Id, balances[0].ResourceId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithUnitFilter_ShouldReturnFilteredResults()
    {
        var resource = await CreateTestResourceAsync();
        var unit1 = await CreateTestUnitOfMeasureAsync("kg");
        var unit2 = await CreateTestUnitOfMeasureAsync("pcs");
        
        var movement1 = StockMovement.Create(resource.Id, unit1.Id, 100, Guid.NewGuid(), MovementType.Receipt);
        var movement2 = StockMovement.Create(resource.Id, unit2.Id, 50, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.AddRange(movement1, movement2);
        await _context.SaveChangesAsync();

        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?unitIds={unit1.Id}");

        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(unit1.Id, balances[0].UnitOfMeasureId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithMultipleFilters_ShouldReturnFilteredResults()
    {
        var resource1 = await CreateTestResourceAsync("Resource 1");
        var resource2 = await CreateTestResourceAsync("Resource 2");
        var unit1 = await CreateTestUnitOfMeasureAsync("kg");
        var unit2 = await CreateTestUnitOfMeasureAsync("pcs");
        
        var movement1 = StockMovement.Create(resource1.Id, unit1.Id, 100, Guid.NewGuid(), MovementType.Receipt);
        var movement2 = StockMovement.Create(resource1.Id, unit2.Id, 50, Guid.NewGuid(), MovementType.Receipt);
        var movement3 = StockMovement.Create(resource2.Id, unit1.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.AddRange(movement1, movement2, movement3);
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/Balance?resourceIds={resource1.Id}&resourceIds={resource2.Id}&unitIds={unit1.Id}");
        var balances = await response.Content.ReadFromJsonAsync<List<BalanceDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(balances);
        Assert.Equal(2, balances.Count);
        Assert.All(balances, b => Assert.Equal(unit1.Id, b.UnitOfMeasureId));
    }

    [Fact]
    public async Task GetBalances_WithNonExistentFilter_ShouldReturnEmptyList()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var movement = StockMovement.Create(resource.Id, unit.Id, 100, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
        
        var nonExistentResourceId = Guid.NewGuid();

        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?resourceIds={nonExistentResourceId}");

        Assert.NotNull(balances);
        Assert.Empty(balances);
    }
}