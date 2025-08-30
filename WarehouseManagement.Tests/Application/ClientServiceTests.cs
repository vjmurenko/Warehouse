using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Domain.Exceptions;

public class ClientServiceTests
{
    private readonly INamedEntityRepository<Client> _namedEntityRepository;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        _namedEntityRepository = Substitute.For<INamedEntityRepository<Client>>();
        _clientService = new ClientService(_namedEntityRepository);
    }

    [Fact]
    public async Task CreateClientWithUniqueNameTest()
    {
        // arrange
        var name = "Test Client";
        var address = "Test Address";
        var guid = Guid.NewGuid();
        
        _namedEntityRepository.CreateAsync(Arg.Is<Client>(c => c.Name == name && c.Address.Name == address)).Returns(guid);
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(false);

        // act
        var result = await _clientService.CreateClientAsync(name, address);

        // assert
        Assert.Equal(guid, result);
    }

    [Fact]
    public async Task CreateClientWithSameNameTest()
    {
        // arrange
        var name = "Test Client";
        var address = "Test Address";
        
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(true);

        // assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _clientService.CreateClientAsync(name, address));
    }

    [Fact]
    public async Task CreateClientWithEmptyNameTest()
    {
        // arrange
        var name = "";
        var address = "Test Address";

        // assert
        await Assert.ThrowsAsync<ArgumentException>(() => _clientService.CreateClientAsync(name, address));
    }

    [Fact]
    public async Task CreateClientWithNullAddressTest()
    {
        // arrange
        var name = "Test Client";
        var address = string.Empty;

        // assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _clientService.CreateClientAsync(name, address));
    }
    

    [Fact]
    public async Task UpdateClientWithValidDataTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Updated Client";
        var address = "Updated Address";
        var client = new Client("Original Client", "Original Address");

        _namedEntityRepository.GetByIdAsync(id).Returns(client);
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(false);
        _namedEntityRepository.UpdateAsync(client).Returns(true);

        // act
        var result = await _clientService.UpdateClientAsync(id, name, address);

        // assert
        Assert.True(result);
        Assert.Equal(name, client.Name);
        Assert.Equal(address, client.Address.Name);
    }

    [Fact]
    public async Task UpdateClientWithNonExistentClientTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Updated Client";
        var address ="Updated Address";

        _namedEntityRepository.GetByIdAsync(id).Returns((Client)null!);

        // act & assert
        var result = await _clientService.UpdateClientAsync(id, name, address);
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateClientWithDuplicateNameTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Duplicate Client";
        var address = "Test Address";
        var client = new Client("Original Client", "Original Address"){Id = id};

        _namedEntityRepository.GetByIdAsync(id).Returns(client);
        _namedEntityRepository.ExistsWithNameAsync(name, id).Returns(true);

        // act & assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _clientService.UpdateClientAsync(id, name, address));
    }
}