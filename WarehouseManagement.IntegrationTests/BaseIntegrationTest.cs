using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Web;

namespace WarehouseManagement.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<TestcontainersWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly TestcontainersWebApplicationFactory<Program> _factory;
    protected readonly IServiceScope _scope;
    protected readonly WarehouseDbContext _context;

    protected BaseIntegrationTest(TestcontainersWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    }
    
    public async Task InitializeAsync()
    {
        // Ensure database is created and clean for each test
        await _context.Database.EnsureCreatedAsync();
        await CleanDatabaseAsync();
    }
    
    public async Task DisposeAsync()
    {
        await CleanDatabaseAsync();
        _scope.Dispose();
        _client.Dispose();
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        return await _client.PostAsJsonAsync(endpoint, data);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        return await _client.PutAsJsonAsync(endpoint, data);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await _client.DeleteAsync(endpoint);
    }

    protected async Task CleanDatabaseAsync()
    {
        // Use database-level delete operations to avoid loading entities
        // Order matters due to foreign key constraints
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ShipmentResources\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ShipmentDocuments\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ReceiptResources\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ReceiptDocuments\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Balances\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Clients\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Resources\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"UnitsOfMeasure\"");
    }

    protected async Task<Resource> CreateTestResourceAsync(string name = "Test Resource")
    {
        var resource = new Resource(name);
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    protected async Task<UnitOfMeasure> CreateTestUnitOfMeasureAsync(string name = "Test Unit")
    {
        var unit = new UnitOfMeasure(name);
        _context.UnitsOfMeasure.Add(unit);
        await _context.SaveChangesAsync();
        return unit;
    }

    protected async Task<Client> CreateTestClientAsync(string name = "Test Client", string addressName = "Test Address")
    {
        var address = new Address(addressName);
        var client = new Client(name, addressName);
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }
}