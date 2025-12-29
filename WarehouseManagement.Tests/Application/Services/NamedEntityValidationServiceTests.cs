using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;


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
        var resource1 = Resource.Create("Resource 1");
        var resource2 = Resource.Create("Resource 2");
        var resourceIds = new[] { resource1.Id, resource2.Id };
        var resources = new List<Resource> { resource1, resource2 };

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
        var resource1 = Resource.Create("Resource 1");
        var missingResourceId = Guid.NewGuid();
        var resourceIds = new[] { resource1.Id, missingResourceId };
        var resources = new List<Resource> { resource1 };

        _resourceRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(resources);

        // Act
        var action = async () => await _validationService.ValidateResourcesAsync(resourceIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Не найдены ресурсы с ID: {missingResourceId}");
    }

    [Fact]
    public async Task validate_resources_should_throw_exception_when_resource_is_archived()
    {
        // Arrange
        var archivedResource = Resource.Create("Archived Resource");
        archivedResource.Archive();
        var resourceIds = new[] { archivedResource.Id };
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
        var resource = Resource.Create("Test Resource");
        var resourceIds = new[] { resource.Id, resource.Id, resource.Id };
        var resources = new List<Resource> { resource };

        _resourceRepository.GetByIdsAsync(Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == resource.Id), Arg.Any<CancellationToken>())
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
        var unit1 = UnitOfMeasure.Create("Kilogram");
        var unit2 = UnitOfMeasure.Create("Piece");
        var unitIds = new[] { unit1.Id, unit2.Id };
        var units = new List<UnitOfMeasure> { unit1, unit2 };

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
        var unit1 = UnitOfMeasure.Create("Kilogram");
        var missingUnitId = Guid.NewGuid();
        var unitIds = new[] { unit1.Id, missingUnitId };
        var units = new List<UnitOfMeasure> { unit1 };

        _unitRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(units);

        // Act
        var action = async () => await _validationService.ValidateUnitsAsync(unitIds, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Не найдены единицы измерения с ID: {missingUnitId}");
    }

    [Fact]
    public async Task validate_units_should_throw_exception_when_unit_is_archived()
    {
        // Arrange
        var archivedUnit = UnitOfMeasure.Create("Archived Unit");
        archivedUnit.Archive();
        var unitIds = new[] { archivedUnit.Id };
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
}
