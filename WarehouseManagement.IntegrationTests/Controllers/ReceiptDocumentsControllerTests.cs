using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class ReceiptDocumentsControllerTests : BaseIntegrationTest
{
    public ReceiptDocumentsControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetReceipts_ShouldReturnEmptyList_WhenNoReceipts()
    {
        // Act
        var receipts = await GetAsync<List<ReceiptDocumentSummaryDto>>("/api/ReceiptDocuments");

        // Assert
        Assert.NotNull(receipts);
        Assert.Empty(receipts);
    }

    [Fact]
    public async Task CreateReceipt_ShouldCreateReceipt_WhenValidData()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var command = new CreateReceiptCommand(
            Number: "REC-001",
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ReceiptResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ReceiptDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var receiptId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, receiptId);

        // Verify receipt was created
        var receipt = await GetAsync<ReceiptDocumentDto>($"/api/ReceiptDocuments/{receiptId}");
        Assert.NotNull(receipt);
        Assert.Equal("REC-001", receipt.Number);
        Assert.Single(receipt.Resources);
        Assert.Equal(resource.Id, receipt.Resources[0].ResourceId);
        Assert.Equal(100, receipt.Resources[0].Quantity);
    }

    [Fact]
    public async Task CreateReceipt_ShouldReturnBadRequest_WhenInvalidResource()
    {
        // Arrange
        var nonExistentResourceId = Guid.NewGuid();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var command = new CreateReceiptCommand(
            Number: "REC-002",
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ReceiptResourceDto(nonExistentResourceId, unit.Id, 100)
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ReceiptDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateReceipt_ShouldReturnBadRequest_WhenInvalidUnitOfMeasure()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var nonExistentUnitId = Guid.NewGuid();
        var client = await CreateTestClientAsync();

        var command = new CreateReceiptCommand(
            Number: "REC-003",
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ReceiptResourceDto(resource.Id, nonExistentUnitId, 100)
            }.ToList()
        );

        // Act
        var response = await PostAsync("/api/ReceiptDocuments", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReceiptById_ShouldReturnReceipt_WhenReceiptExists()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var command = new CreateReceiptCommand(
            Number: "REC-005",
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ReceiptResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        var createResponse = await PostAsync("/api/ReceiptDocuments", command);
        var receiptId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var receipt = await GetAsync<ReceiptDocumentDto>($"/api/ReceiptDocuments/{receiptId}");

        // Assert
        Assert.NotNull(receipt);
        Assert.Equal("REC-005", receipt.Number);
        Assert.Single(receipt.Resources);
    }

    [Fact]
    public async Task GetReceiptById_ShouldReturnNotFound_WhenReceiptDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ReceiptDocuments/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteReceipt_ShouldDeleteReceipt_WhenReceiptExists()
    {
        // Arrange - Create receipt first
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var command = new CreateReceiptCommand(
            Number: "REC-007",
            Date: DateTime.UtcNow,
            Resources: new[]
            {
                new ReceiptResourceDto(resource.Id, unit.Id, 100)
            }.ToList()
        );

        var createResponse = await PostAsync("/api/ReceiptDocuments", command);
        var receiptId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var deleteResponse = await DeleteAsync($"/api/ReceiptDocuments/{receiptId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/ReceiptDocuments/{receiptId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetReceipts_WithDateFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var resource = await CreateTestResourceAsync();
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var date1 = DateTime.UtcNow.AddDays(-2);
        var date2 = DateTime.UtcNow;

        // Create two receipts with different dates
        var command1 = new CreateReceiptCommand(
            Number: "REC-008",
            Date: date1,
            Resources: new[] { new ReceiptResourceDto(resource.Id, unit.Id, 100) }.ToList()
        );

        var command2 = new CreateReceiptCommand(
            Number: "REC-009",
            Date: date2,
            Resources: new[] { new ReceiptResourceDto(resource.Id, unit.Id, 200) }.ToList()
        );

        await PostAsync("/api/ReceiptDocuments", command1);
        await PostAsync("/api/ReceiptDocuments", command2);

        // Act - Filter by date range
        var fromDate = DateTime.UtcNow.AddDays(-1);
        var toDate = DateTime.UtcNow.AddDays(1);
        var receipts = await GetAsync<List<ReceiptDocumentSummaryDto>>(
            $"/api/ReceiptDocuments?fromDate={fromDate:yyyy-MM-ddTHH:mm:ssZ}&toDate={toDate:yyyy-MM-ddTHH:mm:ssZ}");

        // Assert
        Assert.NotNull(receipts);
        Assert.Single(receipts);
        Assert.Equal("REC-009", receipts[0].Number);
    }

    [Fact]
    public async Task GetReceipts_WithResourceFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var resource1 = await CreateTestResourceAsync("Resource 1");
        var resource2 = await CreateTestResourceAsync("Resource 2");
        var unit = await CreateTestUnitOfMeasureAsync("kg");
        var client = await CreateTestClientAsync();

        var command1 = new CreateReceiptCommand(
            Number: "REC-010",
            Date: DateTime.UtcNow,
            Resources: new[] { new ReceiptResourceDto(resource1.Id, unit.Id, 100) }.ToList()
        );

        var command2 = new CreateReceiptCommand(
            Number: "REC-011",
            Date: DateTime.UtcNow,
            Resources: new[] { new ReceiptResourceDto(resource2.Id, unit.Id, 200) }.ToList()
        );

        await PostAsync("/api/ReceiptDocuments", command1);
        await PostAsync("/api/ReceiptDocuments", command2);

        // Act
        var receipts = await GetAsync<List<ReceiptDocumentSummaryDto>>(
            $"/api/ReceiptDocuments?resourceIds={resource1.Id}");

        // Assert
        Assert.NotNull(receipts);
        Assert.Single(receipts);
        Assert.Equal("REC-010", receipts[0].Number);
    }
}