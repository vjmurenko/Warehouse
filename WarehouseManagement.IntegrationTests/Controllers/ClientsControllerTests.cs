using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Features.References.DTOs.Client;
using WarehouseManagement.IntegrationTests.Infrastructure;
using WarehouseManagement.Web;
using CreateClientRequest = WarehouseManagement.Application.Features.References.DTOs.Client.CreateClientRequest;
using UpdateClientRequest = WarehouseManagement.Application.Features.References.DTOs.Client.UpdateClientRequest;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class ClientsControllerTests : BaseIntegrationTest
{
    public ClientsControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClients_ShouldReturnEmptyList_WhenNoClients()
    {
        // Act
        var clients = await GetAsync<List<ClientDto>>("/api/Clients");

        // Assert
        Assert.NotNull(clients);
        Assert.Empty(clients);
    }

    [Fact]
    public async Task CreateClient_ShouldReturnCreatedClient()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Test Client", "123 Test Street");

        // Act
        var response = await PostAsync("/api/Clients", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var clientId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, clientId);
    }

    [Fact]
    public async Task GetClientById_ShouldReturnClient_WhenExists()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Test Client", "123 Test Street");
        var createResponse = await PostAsync("/api/Clients", createRequest);
        var clientId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var client = await GetAsync<ClientDto>($"/api/Clients/{clientId}");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(clientId, client.Id);
        Assert.Equal("Test Client", client.Name);
        Assert.Equal("123 Test Street", client.Address);
        Assert.True(client.IsActive);
    }

    [Fact]
    public async Task GetClientById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Clients/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_ShouldUpdateSuccessfully()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Original Client", "Original Address");
        var createResponse = await PostAsync("/api/Clients", createRequest);
        var clientId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateRequest = new UpdateClientRequest("Updated Client", "Updated Address");

        // Act
        var updateResponse = await PutAsync($"/api/Clients/{clientId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify update
        var updatedClient = await GetAsync<ClientDto>($"/api/Clients/{clientId}");
        Assert.NotNull(updatedClient);
        Assert.Equal("Updated Client", updatedClient.Name);
        Assert.Equal("Updated Address", updatedClient.Address);
    }

    [Fact]
    public async Task DeleteClient_ShouldDeleteSuccessfully()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Client to Delete", "Delete Address");
        var createResponse = await PostAsync("/api/Clients", createRequest);
        var clientId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var deleteResponse = await DeleteAsync($"/api/Clients/{clientId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/Clients/{clientId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ArchiveClient_ShouldArchiveSuccessfully()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Client to Archive", "Archive Address");
        var createResponse = await PostAsync("/api/Clients", createRequest);
        var clientId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var archiveResponse = await _client.PostAsync($"/api/Clients/{clientId}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        // Verify archive
        var archivedClient = await GetAsync<ClientDto>($"/api/Clients/{clientId}");
        Assert.NotNull(archivedClient);
        Assert.False(archivedClient.IsActive);
    }

    [Fact]
    public async Task GetActiveClients_ShouldReturnOnlyActiveClients()
    {
        // Arrange
        var activeRequest = new CreateClientRequest("Active Client", "Active Address");
        var activeResponse = await PostAsync("/api/Clients", activeRequest);
        var activeClientId = await activeResponse.Content.ReadFromJsonAsync<Guid>();

        var inactiveRequest = new CreateClientRequest("Inactive Client", "Inactive Address");
        var inactiveResponse = await PostAsync("/api/Clients", inactiveRequest);
        var inactiveClientId = await inactiveResponse.Content.ReadFromJsonAsync<Guid>();

        // Archive the inactive client
        await _client.PostAsync($"/api/Clients/{inactiveClientId}/archive", null);

        // Act
        var activeClients = await GetAsync<List<ClientDto>>("/api/Clients/active");

        // Assert
        Assert.NotNull(activeClients);
        Assert.Single(activeClients);
        Assert.Equal(activeClientId, activeClients[0].Id);
        Assert.True(activeClients[0].IsActive);
    }
}