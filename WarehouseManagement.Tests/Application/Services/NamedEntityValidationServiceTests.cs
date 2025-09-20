using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Tests.TestBuilders;

namespace WarehouseManagement.Tests.Application.Services;

public class NamedEntityValidationServiceTests
{
    private readonly INamedEntityRepository<Resource> _resourceRepository;
    private readonly INamedEntityRepository<UnitOfMeasure> _unitRepository;
    private readonly ILogger<NamedEntityValidationService> _logger;
    private readonly NamedEntityValidationService _validationService;

    public NamedEntityValidationServiceTests()
    {
        _resourceRepository = Substitute.For<INamedEntityRepository<Resource>>();
        _unitRepository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _logger = Substitute.For<ILogger<NamedEntityValidationService>>();
        
        _validationService = new NamedEntityValidationService(
            _resourceRepository,
            _unitRepository,
            _logger);
    }

    [Fact]
    public async Task validate_resources_should_not_throw_when_all_resources_exist_and_active()
    {
        // Arrange
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var resourceIds = new[] { resourceId1, resourceId2 };

        var resources = new List<Resource>
        {
            TestDataBuilders.Resource().WithName("Resource 1").Build(),
            TestDataBuilders.Resource().WithName("Resource 2").Build()
        };

        // Set the IDs using reflection to match the expected IDs
        SetEntityId(resources[0], resourceId1);
        SetEntityId(resources[1], resourceId2);

        _resourceRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(resources);

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task validate_resources_should_throw_exception_when_resource_not_found()
    {
        // Arrange
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var resourceIds = new[] { resourceId1, resourceId2 };

        var resources = new List<Resource>
        {
            TestDataBuilders.Resource().WithName("Resource 1").Build()
        };
        SetEntityId(resources[0], resourceId1);

        _resourceRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(resources);

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Не найдены ресурсы с ID: {resourceId2}");
    }

    [Fact]
    public async Task validate_resources_should_throw_exception_when_resource_is_archived()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var resourceIds = new[] { resourceId };

        var archivedResource = TestDataBuilders.Resource().WithName("Archived Resource").Build();
        archivedResource.Archive();
        SetEntityId(archivedResource, resourceId);

        var resources = new List<Resource> { archivedResource };

        _resourceRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(resources);

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Следующие ресурсы архивированы и не могут быть использованы: Archived Resource");
    }

    [Fact]
    public async Task validate_resources_should_handle_empty_resource_ids()
    {
        // Arrange
        var resourceIds = Array.Empty<Guid>();

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        await _resourceRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task validate_resources_should_handle_duplicate_resource_ids()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var resourceIds = new[] { resourceId, resourceId, resourceId };

        var resource = TestDataBuilders.Resource().WithName("Test Resource").Build();
        SetEntityId(resource, resourceId);

        var resources = new List<Resource> { resource };

        _resourceRepository.GetByIdsAsync(Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == resourceId), Arg.Any<CancellationToken>())
            .Returns(resources);

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task validate_units_should_not_throw_when_all_units_exist_and_active()
    {
        // Arrange
        var unitId1 = Guid.NewGuid();
        var unitId2 = Guid.NewGuid();
        var unitIds = new[] { unitId1, unitId2 };

        var units = new List<UnitOfMeasure>
        {
            TestDataBuilders.UnitOfMeasure().WithName("Kilogram").Build(),
            TestDataBuilders.UnitOfMeasure().WithName("Piece").Build()
        };

        SetEntityId(units[0], unitId1);
        SetEntityId(units[1], unitId2);

        _unitRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(units);

        // Act
        var action = async () => await _validationService.ValidateUnitsAsync(unitIds, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task validate_units_should_throw_exception_when_unit_not_found()
    {
        // Arrange
        var unitId1 = Guid.NewGuid();
        var unitId2 = Guid.NewGuid();
        var unitIds = new[] { unitId1, unitId2 };

        var units = new List<UnitOfMeasure>
        {
            TestDataBuilders.UnitOfMeasure().WithName("Kilogram").Build()
        };
        SetEntityId(units[0], unitId1);

        _unitRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(units);

        // Act
        var action = async () => await _validationService.ValidateUnitsAsync(unitIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Не найдены единицы измерения с ID: {unitId2}");
    }

    [Fact]
    public async Task validate_units_should_throw_exception_when_unit_is_archived()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unitIds = new[] { unitId };

        var archivedUnit = TestDataBuilders.UnitOfMeasure().WithName("Archived Unit").Build();
        archivedUnit.Archive();
        SetEntityId(archivedUnit, unitId);

        var units = new List<UnitOfMeasure> { archivedUnit };

        _unitRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(units);

        // Act
        var action = async () => await _validationService.ValidateUnitsAsync(unitIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Следующие единицы измерения архивированы и не могут быть использованы: Archived Unit");
    }

    [Fact]
    public async Task validate_units_should_handle_empty_unit_ids()
    {
        // Arrange
        var unitIds = Array.Empty<Guid>();

        // Act
        var action = async () => await _validationService.ValidateUnitsAsync(unitIds, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
        await _unitRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        idProperty?.SetValue(entity, id);
    }
}