using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class BalanceControllerTests : BaseIntegrationTest
{
    public BalanceControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBalances_ShouldReturnEmptyList_WhenNoBalances()
    {
        // Act
        var balances = await GetAsync<List<BalanceDto>>("/api/Balance");

        // Assert
        Assert.NotNull(balances);
        Assert.Empty(balances);
    }

    [Fact]
    public async Task GetBalances_ShouldReturnBalances_WhenBalancesExist()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        
        var balance = new Balance(resource.Id, unit.Id, new Quantity(100));
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        // Act
        var balances = await GetAsync<List<BalanceDto>>("/api/Balance");

        // Assert
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(resource.Id, balances[0].ResourceId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithResourceFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var resource1 = await CreateTestResourceAsync("Resource 1");
        var resource2 = await CreateTestResourceAsync("Resource 2");
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        
        var balance1 = new Balance(resource1.Id, unit.Id, new Quantity(100));
        var balance2 = new Balance(resource2.Id, unit.Id, new Quantity(200));
        _context.Balances.AddRange(balance1, balance2);
        await _context.SaveChangesAsync();

        // Act
        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?resourceIds={resource1.Id}");

        // Assert
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(resource1.Id, balances[0].ResourceId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithUnitFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit1 = await CreateTestUnitOfMeasureAsync("kg");
        var unit2 = await CreateTestUnitOfMeasureAsync("pcs");
        
        var balance1 = new Balance(resource.Id, unit1.Id, new Quantity(100));
        var balance2 = new Balance(resource.Id, unit2.Id, new Quantity(50));
        _context.Balances.AddRange(balance1, balance2);
        await _context.SaveChangesAsync();

        // Act
        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?unitIds={unit1.Id}");

        // Assert
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(unit1.Id, balances[0].UnitOfMeasureId);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task GetBalances_WithMultipleFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var resource1 = await CreateTestResourceAsync("Resource 1");
        var resource2 = await CreateTestResourceAsync("Resource 2");
        var unit1 = await CreateTestUnitOfMeasureAsync("kg");
        var unit2 = await CreateTestUnitOfMeasureAsync("pcs");
        
        var balance1 = new Balance(resource1.Id, unit1.Id, new Quantity(100));
        var balance2 = new Balance(resource1.Id, unit2.Id, new Quantity(50));
        var balance3 = new Balance(resource2.Id, unit1.Id, new Quantity(200));
        _context.Balances.AddRange(balance1, balance2, balance3);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/Balance?resourceIds={resource1.Id}&resourceIds={resource2.Id}&unitIds={unit1.Id}");
        var balances = await response.Content.ReadFromJsonAsync<List<BalanceDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(balances);
        Assert.Equal(2, balances.Count);
        Assert.All(balances, b => Assert.Equal(unit1.Id, b.UnitOfMeasureId));
    }

    [Fact]
    public async Task GetBalances_WithNonExistentFilter_ShouldReturnEmptyList()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var balance = new Balance(resource.Id, unit.Id, new Quantity(100));
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();
        
        var nonExistentResourceId = Guid.NewGuid();

        // Act
        var balances = await GetAsync<List<BalanceDto>>($"/api/Balance?resourceIds={nonExistentResourceId}");

        // Assert
        Assert.NotNull(balances);
        Assert.Empty(balances);
    }
}