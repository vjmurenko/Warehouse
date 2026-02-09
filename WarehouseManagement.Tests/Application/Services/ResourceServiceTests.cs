using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.SharedKernel.Exceptions;


namespace WarehouseManagement.Tests.Application.Services;

public class ResourceServiceTests
{
    private readonly IReferenceRepository<Resource> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NamedEntityService<Resource>> _logger;
    private readonly TestableResourceService _service;

    public ResourceServiceTests()
    {
        _repository = Substitute.For<IReferenceRepository<Resource>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<NamedEntityService<Resource>>>();
        
        _service = new TestableResourceService(_repository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task create_async_should_throw_exception_when_name_already_exists()
    {
        // Arrange
        var resource = Resource.Create("Existing Resource");
        _repository.ExistsWithNameAsync(resource.Name, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.CreateAsync(resource, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Resource with name 'Existing Resource' already exists");
        
        _repository.DidNotReceive().Create(Arg.Any<Resource>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task update_async_should_update_entity_when_new_name_is_unique()
    {
        // Arrange
        var resource = Resource.Create("Updated Resource");

        _repository.ExistsWithNameAsync(resource.Name, resource.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.UpdateAsync(resource, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(resource);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task update_async_should_throw_exception_when_new_name_already_exists()
    {
        // Arrange
        var resource = Resource.Create("Existing Resource");

        _repository.ExistsWithNameAsync(resource.Name, resource.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.UpdateAsync(resource, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("Resource with name 'Existing Resource' already exists");
        
        _repository.DidNotReceive().Update(Arg.Any<Resource>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_delete_entity_when_entity_exists_and_not_in_use()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = Resource.Create("Test Resource");

        _repository.IsUsingInDocuments(entityId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(entityId, Arg.Any<CancellationToken>())
            .Returns(existingEntity);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.DeleteAsync(entityId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Delete(existingEntity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_throw_exception_when_entity_is_in_use()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        _repository.IsUsingInDocuments(entityId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.DeleteAsync(entityId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityInUseException>()
            .WithMessage($"Cannot delete Resource with ID {entityId} because it is used in documents");
        
        _repository.DidNotReceive().Delete(Arg.Any<Resource>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_throw_exception_when_entity_not_found()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        _repository.IsUsingInDocuments(entityId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(entityId, Arg.Any<CancellationToken>())
            .Returns((Resource?)null);

        // Act
        var action = async () => await _service.DeleteAsync(entityId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Resource with ID {entityId} was not found");
        
        _repository.DidNotReceive().Delete(Arg.Any<Resource>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task archive_async_should_archive_entity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ArchiveAsync(entityId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ArchiveAsync(entityId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task activate_async_should_activate_entity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ActivateAsync(entityId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ActivateAsync(entityId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// Test-specific implementation to make the abstract class testable
public class TestableResourceService : NamedEntityService<Resource>
{
    public TestableResourceService(IReferenceRepository<Resource> repository, IUnitOfWork unitOfWork, ILogger<NamedEntityService<Resource>> logger)
        : base(repository, unitOfWork, logger)
    {
    }
}
