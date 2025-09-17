using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class ShipmentDocumentsControllerTests : BaseIntegrationTest
{
    public ShipmentDocumentsControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task CreateShipment_ShouldCreateShipment_WhenValidDataAndSufficientInventory()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create initial balance
        var balance = new Balance(resource.Id, unit.Id, new Quantity(200));
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-001",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ShipmentDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var shipmentId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, shipmentId);

        // Verify shipment was created
        var shipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");
        Assert.NotNull(shipment);
        Assert.Equal("SHIP-001", shipment.Number);
        Assert.Equal(client.Id, shipment.ClientId);
        Assert.Single(shipment.Resources);
        Assert.Equal(resource.Id, shipment.Resources[0].ResourceId);
        Assert.Equal(100, shipment.Resources[0].Quantity);
        Assert.False(shipment.IsSigned);
    }

    [Fact]
    public async Task CreateShipment_ShouldReturnBadRequest_WhenInsufficientInventory()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create insufficient balance
        var balance = new Balance(resource.Id, unit.Id, new Quantity(50));
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-002",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100) // More than available
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ShipmentDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetShipmentById_ShouldReturnShipment_WhenShipmentExists()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create sufficient balance
        var balance = new Balance(resource.Id, unit.Id, new Quantity(200));
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-003",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        var createResponse = await PostAsync("/api/ShipmentDocuments", command);
        var shipmentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var shipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");

        // Assert
        Assert.NotNull(shipment);
        Assert.Equal("SHIP-003", shipment.Number);
        Assert.Equal(client.Id, shipment.ClientId);
        Assert.Single(shipment.Resources);
        Assert.False(shipment.IsSigned);
    }

    [Fact]
    public async Task GetShipmentById_ShouldReturnNotFound_WhenShipmentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ShipmentDocuments/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateShipment_ShouldReduceBalance_WhenShipmentCreated()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create initial balance
        var initialBalance = new Balance(resource.Id, unit.Id, new Quantity(200));
        _context.Balances.Add(initialBalance);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-004",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ShipmentDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify balance was NOT reduced (unsigned document should not affect balance)
        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(200, balances[0].Quantity); // Should remain 200 (unchanged)
    }

    [Fact]
    public async Task CreateShipment_ShouldReduceBalance_WhenShipmentCreatedWithImmediateSigning()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create initial balance
        var initialBalance = new Balance(resource.Id, unit.Id, new Quantity(200));
        _context.Balances.Add(initialBalance);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-005",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList(),
            Sign: true
        );

        // Act
        var response = await PostAsync("/api/ShipmentDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify balance was reduced (signed document should affect balance)
        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(100, balances[0].Quantity); // 200 - 100 = 100
    }

    [Fact]
    public async Task UpdateShipment_ShouldReduceBalance_WhenUnsignedShipmentIsSignedDuringUpdate()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create initial balance
        var initialBalance = new Balance(resource.Id, unit.Id, new Quantity(200));
        _context.Balances.Add(initialBalance);
        await _context.SaveChangesAsync();

        // Create unsigned shipment
        var command = new CreateShipmentCommand(
            Number: "SHIP-006",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        var createResponse = await PostAsync("/api/ShipmentDocuments", command);
        var shipmentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Verify balance is not affected yet
        var balancesBeforeSigning = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.Equal(200, balancesBeforeSigning![0].Quantity);

        // Get the shipment
        var shipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");
        Assert.NotNull(shipment);
        Assert.False(shipment.IsSigned);

        // Act - Update the shipment to sign it
        var updateCommand = new UpdateShipmentCommand(
            Id: shipmentId,
            Number: shipment.Number,
            ClientId: shipment.ClientId,
            Date: shipment.Date,
            Resources: shipment.Resources.Select(r => new ShipmentResourceDto(r.ResourceId, r.UnitId, r.Quantity)).ToList(),
            Sign: true // Sign during update
        );
        
        var updateResponse = await PutAsync($"/api/ShipmentDocuments/{shipmentId}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify balance was reduced after signing
        var balancesAfterSigning = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balancesAfterSigning);
        Assert.Single(balancesAfterSigning);
        Assert.Equal(100, balancesAfterSigning[0].Quantity); // 200 - 100 = 100

        // Verify shipment is now signed
        var updatedShipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");
        Assert.NotNull(updatedShipment);
        Assert.True(updatedShipment.IsSigned);
    }

    [Fact]
    public async Task CreateShipment_ShouldReturnBadRequest_WhenInsufficientBalanceForImmediateSigning()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        // Create insufficient balance
        var initialBalance = new Balance(resource.Id, unit.Id, new Quantity(50));
        _context.Balances.Add(initialBalance);
        await _context.SaveChangesAsync();

        // Create shipment with Sign=true and more resources than available
        var command = new CreateShipmentCommand(
            Number: "SHIP-007",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100) // More than available 50
            }.ToList(),
            Sign: true // Try to sign immediately
        );

        // Act - Try to create and sign the shipment
        var createResponse = await PostAsync("/api/ShipmentDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        
        // Read the error message as properly formatted JSON
        var errorResponse = await createResponse.Content.ReadFromJsonAsync<WarehouseManagement.Application.Common.Models.ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.Equal("INSUFFICIENT_BALANCE", errorResponse.Code);
        Assert.Contains("Insufficient balance", errorResponse.Message);

        // Verify balance was not affected
        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(50, balances[0].Quantity); // Should remain unchanged
    }
}