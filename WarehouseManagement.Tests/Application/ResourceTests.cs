using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;

public class ResourceTests
{
    private readonly INamedEntityRepository<Resource> _namedEntityRepository;
    private readonly ResourceService _resourceService;

    public ResourceTests()
    {
        _namedEntityRepository = Substitute.For<INamedEntityRepository<Resource>>();
        _resourceService = new ResourceService(_namedEntityRepository);
    }


    [Fact]
    public async Task CreateResourceWithUniqueNameTest()
    {
        //arrange
        var name = "name";
        var guid = Guid.NewGuid();
        _namedEntityRepository.Create(Arg.Is<Resource>(c => c.Name == name)).Returns(guid);
        _namedEntityRepository.ExistsWithNameAsync("name").Returns(false);

        //act
        var result = await _resourceService.CreateResourceAsync(name);

        //assert
        Assert.Equal(guid, result);
    }


    [Fact]
    public async Task CreateResourceWithSameNameTest()
    {
        //arrange
        var name = "name";
        
        _namedEntityRepository.ExistsWithNameAsync("name").Returns(true);

        //act & assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _resourceService.CreateResourceAsync(name));
    }

    [Fact]
    public async Task DeleteResourceWithValidDataTest()
    {
        //arrange
        var guid = Guid.NewGuid();
        
        _namedEntityRepository.IsUsingInDocuments(guid).Returns(false);
        _namedEntityRepository.GetByIdAsync(guid).Returns(new Resource("abc"));
        _namedEntityRepository.Delete(Arg.Any<Resource>()).Returns(true);
        
        //act
        var result = await _resourceService.DeleteAsync(guid);
        
        //assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateResourceWithValidDataTest()
    {
        //arrange
        var id = Guid.NewGuid();
        var name = "updated name";
        var resource = new Resource("original name");

        _namedEntityRepository.GetByIdAsync(id).Returns(resource);
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(false);
        _namedEntityRepository.Update(resource).Returns(true);

        //act
        var result = await _resourceService.UpdateResourceAsync(id, name);

        //assert
        Assert.True(result);
        Assert.Equal(name, resource.Name);
    }

    [Fact]
    public async Task UpdateResourceWithNonExistentResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();
        var name = "updated name";

        _namedEntityRepository.GetByIdAsync(id).Returns((Resource)null!);

        //act & assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _resourceService.UpdateResourceAsync(id, name));
    }

    [Fact]
    public async Task UpdateResourceWithDuplicateNameTest()
    {
        //arrange
        var id = Guid.NewGuid();
        var name = "duplicate name";
        var resource = new Resource("original name"){Id = id};

        _namedEntityRepository.GetByIdAsync(id).Returns(resource);
        _namedEntityRepository.ExistsWithNameAsync(name, id).Returns(true);

        //act & assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _resourceService.UpdateResourceAsync(id, name));
    }

    [Fact]
    public async Task GetAllResourcesTest()
    {
        //arrange
        var resources = new List<Resource>
        {
            new ("Resource 1"),
            new ("Resource 2"),
            new ("Resource 3")
        };

        _namedEntityRepository.GetAllAsync().Returns(resources);

        //act
        var result = await _resourceService.GetAllAsync();

        //assert
        Assert.Equal(resources, result);
    }

    [Fact]
    public async Task GetActiveResourcesTest()
    {
        //arrange
        var resources = new List<Resource>
        {
            new ("Active Resource 1"),
            new ("Active Resource 2")
        };

        _namedEntityRepository.GetActiveAsync().Returns(resources);

        //act
        var result = await _resourceService.GetActiveAsync();

        //assert
        Assert.Equal(resources, result);
    }

    [Fact]
    public async Task GetArchivedResourcesTest()
    {
        //arrange
        var resources = new List<Resource>
        {
            new("Archived Resource 1"),
            new("Archived Resource 2")
        };

        _namedEntityRepository.GetArchivedAsync().Returns(resources);

        //act
        var result = await _resourceService.GetArchivedAsync();

        //assert
        Assert.Equal(resources, result);
    }

    [Fact]
    public async Task GetResourceByIdWithExistingResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();
        var resource = new Resource("existing resource");

        _namedEntityRepository.GetByIdAsync(id).Returns(resource);

        //act
        var result = await _resourceService.GetByIdAsync(id);

        //assert
        Assert.Equal(resource, result);
    }

    [Fact]
    public async Task GetResourceByIdWithNonExistentResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();

        _namedEntityRepository.GetByIdAsync(id).Returns((Resource)null!);

        //act
        var result = await _resourceService.GetByIdAsync(id);

        //assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();

        _namedEntityRepository.ArchiveAsync(id).Returns(true);

        //act
        var result = await _resourceService.ArchiveAsync(id);

        //assert
        Assert.True(result);
    }

    [Fact]
    public async Task ActivateResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();

        _namedEntityRepository.ActivateAsync(id).Returns(true);

        //act
        var result = await _resourceService.ActivateAsync(id);

        //assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteResourceWithNonExistentResourceTest()
    {
        //arrange
        var id = Guid.NewGuid();

        _namedEntityRepository.GetByIdAsync(id).Returns((Resource)null!);

        //act & assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _resourceService.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteResourceWithResourceUsedInDocumentsTest()
    {
        //arrange
        var id = Guid.NewGuid();

        _namedEntityRepository.IsUsingInDocuments(id).Returns(true);

        //act & assert
        await Assert.ThrowsAsync<EntityInUseException>(() => _resourceService.DeleteAsync(id));
    }
}