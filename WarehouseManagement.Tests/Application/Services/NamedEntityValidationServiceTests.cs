using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Tests.Application.Services;

public class NamedEntityValidationServiceTests
{
    private readonly INamedEntityRepository<Resource> _resourceRepository;
    private readonly INamedEntityRepository<UnitOfMeasure> _unitRepository;
    private readonly NamedEntityValidationService _validationService;

    public NamedEntityValidationServiceTests()
    {
        _resourceRepository = Substitute.For<INamedEntityRepository<Resource>>();
        _unitRepository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _validationService = new NamedEntityValidationService(_resourceRepository, _unitRepository);
    }

    [Fact]
    public async Task ValidateResourceAsync_WithValidActiveResource_ShouldReturnResource()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var resource = new Resource("Test Resource") { Id = resourceId };
        _resourceRepository.GetByIdAsync(resourceId).Returns(resource);

        // Act
        var result = await _validationService.ValidateResourceAsync(resourceId, CancellationToken.None);

        // Assert
        Assert.Equal(resource, result);
        Assert.Equal(resourceId, result.Id);
        Assert.Equal("Test Resource", result.Name);
    }

    [Fact]
    public async Task ValidateResourceAsync_WithNonExistentResource_ShouldThrowArgumentException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        _resourceRepository.GetByIdAsync(resourceId).Returns((Resource?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _validationService.ValidateResourceAsync(resourceId, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {resourceId} не найден", exception.Message);
        Assert.Equal("resourceId", exception.ParamName);
    }

    [Fact]
    public async Task ValidateResourceAsync_WithArchivedResource_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var resource = new Resource("Archived Resource") { Id = resourceId };
        resource.Archive(); // Архивируем ресурс
        _resourceRepository.GetByIdAsync(resourceId).Returns(resource);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validationService.ValidateResourceAsync(resourceId, CancellationToken.None));
        
        Assert.Contains($"Ресурс 'Archived Resource' архивирован и не может быть использован", exception.Message);
    }

    [Fact]
    public async Task ValidateUnitOfMeasureAsync_WithValidActiveUnit_ShouldReturnUnit()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = new UnitOfMeasure("Test Unit") { Id = unitId };
        _unitRepository.GetByIdAsync(unitId).Returns(unit);

        // Act
        var result = await _validationService.ValidateUnitOfMeasureAsync(unitId, CancellationToken.None);

        // Assert
        Assert.Equal(unit, result);
        Assert.Equal(unitId, result.Id);
        Assert.Equal("Test Unit", result.Name);
    }

    [Fact]
    public async Task ValidateUnitOfMeasureAsync_WithNonExistentUnit_ShouldThrowArgumentException()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        _unitRepository.GetByIdAsync(unitId).Returns((UnitOfMeasure?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _validationService.ValidateUnitOfMeasureAsync(unitId, CancellationToken.None));
        
        Assert.Contains($"Единица измерения с ID {unitId} не найдена", exception.Message);
        Assert.Equal("unitId", exception.ParamName);
    }

    [Fact]
    public async Task ValidateUnitOfMeasureAsync_WithArchivedUnit_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = new UnitOfMeasure("Archived Unit") { Id = unitId };
        unit.Archive(); // Архивируем единицу измерения
        _unitRepository.GetByIdAsync(unitId).Returns(unit);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validationService.ValidateUnitOfMeasureAsync(unitId, CancellationToken.None));
        
        Assert.Contains($"Единица измерения 'Archived Unit' архивирована и не может быть использована", exception.Message);
    }

    [Fact]
    public async Task ValidateResourceAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var resource = new Resource("Test Resource") { Id = resourceId };
        var cancellationToken = new CancellationToken();
        _resourceRepository.GetByIdAsync(resourceId).Returns(resource);

        // Act
        await _validationService.ValidateResourceAsync(resourceId, cancellationToken);

        // Assert
        await _resourceRepository.Received(1).GetByIdAsync(resourceId);
    }

    [Fact]
    public async Task ValidateUnitOfMeasureAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = new UnitOfMeasure("Test Unit") { Id = unitId };
        var cancellationToken = new CancellationToken();
        _unitRepository.GetByIdAsync(unitId).Returns(unit);

        // Act
        await _validationService.ValidateUnitOfMeasureAsync(unitId, cancellationToken);

        // Assert
        await _unitRepository.Received(1).GetByIdAsync(unitId);
    }
}