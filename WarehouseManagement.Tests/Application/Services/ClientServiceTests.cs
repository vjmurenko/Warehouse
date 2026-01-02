using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Tests.Application.Services;

public class ClientServiceTests
{
    private readonly INamedEntityRepository<Client> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NamedEntityService<Client>> _logger;
    private readonly TestableClientService _service;

    public ClientServiceTests()
    {
        _repository = Substitute.For<INamedEntityRepository<Client>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<NamedEntityService<Client>>>();
        
        _service = new TestableClientService(_repository, _unitOfWork, _logger);
    }

    #region Uniqueness Tests

    [Fact]
    public async Task create_async_should_throw_exception_when_name_already_exists()
    {
        // Arrange
        var client = Client.Create("Existing Client", new Address("Test Address"));
        _repository.ExistsWithNameAsync(client.Name, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.CreateAsync(client, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Client with name 'Existing Client' already exists");
        
        _repository.DidNotReceive().Create(Arg.Any<Client>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task update_async_should_throw_exception_when_new_name_already_exists()
    {
        // Arrange
        var client = Client.Create("Existing Client", new Address("Test Address"));

        _repository.ExistsWithNameAsync(client.Name, client.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.UpdateAsync(client, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Client with name 'Existing Client' already exists");
        
        _repository.DidNotReceive().Update(Arg.Any<Client>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Archiving Tests

    [Fact]
    public async Task delete_async_should_throw_exception_when_client_is_in_use()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        _repository.IsUsingInDocuments(clientId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.DeleteAsync(clientId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityInUseException>()
            .WithMessage($"Cannot delete Client with ID {clientId} because it is used in documents");
        
        _repository.DidNotReceive().Delete(Arg.Any<Client>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task archive_async_should_archive_client_when_in_use()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ArchiveAsync(clientId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ArchiveAsync(clientId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_delete_client_when_not_in_use()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = Client.Create("Test Client", new Address("Test Address"));

        _repository.IsUsingInDocuments(clientId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
            .Returns((Client?)existingClient);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.DeleteAsync(clientId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Delete(existingClient);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Standard Operation Tests

    [Fact]
    public async Task update_async_should_update_client_when_new_name_is_unique()
    {
        // Arrange
        var client = Client.Create("Updated Client", new Address("Updated Address"));

        _repository.ExistsWithNameAsync(client.Name, client.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.UpdateAsync(client, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(client);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_throw_exception_when_client_not_found()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        _repository.IsUsingInDocuments(clientId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        // Act
        var action = async () => await _service.DeleteAsync(clientId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Client with ID {clientId} was not found");
        
        _repository.DidNotReceive().Delete(Arg.Any<Client>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task activate_async_should_activate_client()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ActivateAsync(clientId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ActivateAsync(clientId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}

// Test-specific implementation to make the abstract class testable
public class TestableClientService : NamedEntityService<Client>
{
    public TestableClientService(INamedEntityRepository<Client> repository, IUnitOfWork unitOfWork, ILogger<NamedEntityService<Client>> logger)
        : base(repository, unitOfWork, logger)
    {
    }
}
