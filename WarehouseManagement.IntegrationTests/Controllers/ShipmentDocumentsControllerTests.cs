using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.UpdateShipment;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Web;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class ShipmentDocumentsControllerTests : BaseIntegrationTest
{
    public ShipmentDocumentsControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task CreateShipment_ShouldCreateShipment_WhenValidDataAndSufficientInventory()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
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

        var response = await PostAsync("/api/ShipmentDocuments", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var shipmentId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, shipmentId);

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
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 50, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-002",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList(),
            Sign: true
        );

        var response = await PostAsync("/api/ShipmentDocuments", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetShipmentById_ShouldReturnShipment_WhenShipmentExists()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
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

        var shipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");

        Assert.NotNull(shipment);
        Assert.Equal("SHIP-003", shipment.Number);
        Assert.Equal(client.Id, shipment.ClientId);
        Assert.Single(shipment.Resources);
        Assert.False(shipment.IsSigned);
    }

    [Fact]
    public async Task GetShipmentById_ShouldReturnNotFound_WhenShipmentDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/ShipmentDocuments/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateShipment_ShouldReduceBalance_WhenShipmentCreated()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
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

        var response = await PostAsync("/api/ShipmentDocuments", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(200, balances[0].Quantity);
    }

    [Fact]
    public async Task CreateShipment_ShouldReduceBalance_WhenShipmentCreatedWithImmediateSigning()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
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

        var response = await PostAsync("/api/ShipmentDocuments", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(100, balances[0].Quantity);
    }

    [Fact]
    public async Task UpdateShipment_ShouldReduceBalance_WhenUnsignedShipmentIsSignedDuringUpdate()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 200, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

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

        var balancesBeforeSigning = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.Equal(200, balancesBeforeSigning![0].Quantity);

        var shipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");
        Assert.NotNull(shipment);
        Assert.False(shipment.IsSigned);

        var updateCommand = new UpdateShipmentCommand(
            Id: shipmentId,
            Number: shipment.Number,
            ClientId: shipment.ClientId,
            Date: shipment.Date,
            Resources: shipment.Resources.Select(r => new ShipmentResourceDto(r.ResourceId, r.UnitId, r.Quantity)).ToList(),
            Sign: true
        );
        
        var updateResponse = await PutAsync($"/api/ShipmentDocuments/{shipmentId}", updateCommand);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var balancesAfterSigning = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balancesAfterSigning);
        Assert.Single(balancesAfterSigning);
        Assert.Equal(100, balancesAfterSigning[0].Quantity);

        var updatedShipment = await GetAsync<ShipmentDocumentDto>($"/api/ShipmentDocuments/{shipmentId}");
        Assert.NotNull(updatedShipment);
        Assert.True(updatedShipment.IsSigned);
    }

    [Fact]
    public async Task CreateShipment_ShouldReturnBadRequest_WhenInsufficientBalanceForImmediateSigning()
    {
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var movement = new StockMovement(resource.Id, unit.Id, 50, Guid.NewGuid(), MovementType.Receipt);
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        var command = new CreateShipmentCommand(
            Number: "SHIP-007",
            ClientId: client.Id,
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ShipmentResourceDto(resource.Id, unit.Id, 100)
            }.ToList(),
            Sign: true
        );

        var createResponse = await PostAsync("/api/ShipmentDocuments", command);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        
        var errorResponse = await createResponse.Content.ReadFromJsonAsync<WarehouseManagement.Application.Common.Models.ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.Equal("INSUFFICIENT_BALANCE", errorResponse.Code);
        Assert.Contains("Insufficient balance", errorResponse.Message);

        var balances = await GetAsync<List<WarehouseManagement.Application.Features.Balances.DTOs.BalanceDto>>("/api/Balance");
        Assert.NotNull(balances);
        Assert.Single(balances);
        Assert.Equal(50, balances[0].Quantity);
    }
}
