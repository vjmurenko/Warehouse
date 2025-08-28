using System.Net;
using System.Net.Http.Json;
using WarehouseManagement.Application.Dtos.Resource;
using WarehouseManagement.IntegrationTests.Infrastructure;

namespace WarehouseManagement.IntegrationTests.Controllers;

public class ResourcesControllerTests : BaseIntegrationTest
{
    public ResourcesControllerTests(TestcontainersWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetResources_ShouldReturnEmptyList_WhenNoResources()
    {
        // Act
        var resources = await GetAsync<List<ResourceDto>>("/api/Resources");

        // Assert
        Assert.NotNull(resources);
        Assert.Empty(resources);
    }

    [Fact]
    public async Task CreateResource_ShouldReturnCreatedResource()
    {
        // Arrange
        var createRequest = new CreateResourceRequest("Test Resource");

        // Act
        var response = await PostAsync("/api/Resources", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var resourceId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, resourceId);
    }

    [Fact]
    public async Task GetResourceById_ShouldReturnResource_WhenExists()
    {
        // Arrange
        var createRequest = new CreateResourceRequest("Test Resource");
        var createResponse = await PostAsync("/api/Resources", createRequest);
        var resourceId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var resource = await GetAsync<ResourceDto>($"/api/Resources/{resourceId}");

        // Assert
        Assert.NotNull(resource);
        Assert.Equal(resourceId, resource.Id);
        Assert.Equal("Test Resource", resource.Name);
        Assert.True(resource.IsActive);
    }

    [Fact]
    public async Task GetResourceById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Resources/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateResource_ShouldUpdateSuccessfully()
    {
        // Arrange
        var createRequest = new CreateResourceRequest("Original Name");
        var createResponse = await PostAsync("/api/Resources", createRequest);
        var resourceId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateRequest = new UpdateResourceRequest("Updated Name");

        // Act
        var updateResponse = await PutAsync($"/api/Resources/{resourceId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify update
        var updatedResource = await GetAsync<ResourceDto>($"/api/Resources/{resourceId}");
        Assert.NotNull(updatedResource);
        Assert.Equal("Updated Name", updatedResource.Name);
    }

    [Fact]
    public async Task DeleteResource_ShouldDeleteSuccessfully()
    {
        // Arrange
        var createRequest = new CreateResourceRequest("Resource to Delete");
        var createResponse = await PostAsync("/api/Resources", createRequest);
        var resourceId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var deleteResponse = await DeleteAsync($"/api/Resources/{resourceId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/Resources/{resourceId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ArchiveResource_ShouldArchiveSuccessfully()
    {
        // Arrange
        var createRequest = new CreateResourceRequest("Resource to Archive");
        var createResponse = await PostAsync("/api/Resources", createRequest);
        var resourceId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var archiveResponse = await _client.PostAsync($"/api/Resources/{resourceId}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        // Verify archive
        var archivedResource = await GetAsync<ResourceDto>($"/api/Resources/{resourceId}");
        Assert.NotNull(archivedResource);
        Assert.False(archivedResource.IsActive);
    }

    [Fact]
    public async Task GetActiveResources_ShouldReturnOnlyActiveResources()
    {
        // Arrange
        var activeRequest = new CreateResourceRequest("Active Resource");
        var activeResponse = await PostAsync("/api/Resources", activeRequest);
        var activeResourceId = await activeResponse.Content.ReadFromJsonAsync<Guid>();

        var inactiveRequest = new CreateResourceRequest("Inactive Resource");
        var inactiveResponse = await PostAsync("/api/Resources", inactiveRequest);
        var inactiveResourceId = await inactiveResponse.Content.ReadFromJsonAsync<Guid>();

        // Archive the inactive resource
        await _client.PostAsync($"/api/Resources/{inactiveResourceId}/archive", null);

        // Act
        var activeResources = await GetAsync<List<ResourceDto>>("/api/Resources/active");

        // Assert
        Assert.NotNull(activeResources);
        Assert.Single(activeResources);
        Assert.Equal(activeResourceId, activeResources[0].Id);
        Assert.True(activeResources[0].IsActive);
    }
}