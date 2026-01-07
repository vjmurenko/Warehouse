using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Tests.Application.Services;

public class UnitOfMeasureServiceTests
{
    private readonly INamedEntityRepository<UnitOfMeasure> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NamedEntityService<UnitOfMeasure>> _logger;
    private readonly TestableUnitOfMeasureService _service;

    public UnitOfMeasureServiceTests()
    {
        _repository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<NamedEntityService<UnitOfMeasure>>>();
        
        _service = new TestableUnitOfMeasureService(_repository, _unitOfWork, _logger);
    }

    #region Uniqueness Tests

    [Fact]
    public async Task create_async_should_throw_exception_when_name_already_exists()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Existing Unit");
        _repository.ExistsWithNameAsync(unit.Name, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.CreateAsync(unit, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("UnitOfMeasure with name 'Existing Unit' already exists");
        
        _repository.DidNotReceive().Create(Arg.Any<UnitOfMeasure>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task update_async_should_throw_exception_when_new_name_already_exists()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Existing Unit");

        _repository.ExistsWithNameAsync(unit.Name, unit.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.UpdateAsync(unit, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage("UnitOfMeasure with name 'Existing Unit' already exists");
        
        _repository.DidNotReceive().Update(Arg.Any<UnitOfMeasure>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Archiving Tests

    [Fact]
    public async Task delete_async_should_throw_exception_when_unit_is_in_use()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        _repository.IsUsingInDocuments(unitId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await _service.DeleteAsync(unitId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityInUseException>()
            .WithMessage($"Cannot delete UnitOfMeasure with ID {unitId} because it is used in documents");
        
        _repository.DidNotReceive().Delete(Arg.Any<UnitOfMeasure>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task archive_async_should_archive_unit_when_in_use()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ArchiveAsync(unitId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ArchiveAsync(unitId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_delete_unit_when_not_in_use()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var existingUnit = UnitOfMeasure.Create("Test Unit");

        _repository.IsUsingInDocuments(unitId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns(existingUnit);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.DeleteAsync(unitId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Delete(existingUnit);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Standard Operation Tests

    [Fact]
    public async Task update_async_should_update_unit_when_new_name_is_unique()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Updated Unit");

        _repository.ExistsWithNameAsync(unit.Name, unit.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.UpdateAsync(unit, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Received(1).Update(unit);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task delete_async_should_throw_exception_when_unit_not_found()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        _repository.IsUsingInDocuments(unitId, Arg.Any<CancellationToken>())
            .Returns(false);
        _repository.GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns((UnitOfMeasure?)null);

        // Act
        var action = async () => await _service.DeleteAsync(unitId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"UnitOfMeasure with ID {unitId} was not found");
        
        _repository.DidNotReceive().Delete(Arg.Any<UnitOfMeasure>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task activate_async_should_activate_unit()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.ActivateAsync(unitId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).ActivateAsync(unitId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}

// Test-specific implementation to make the abstract class testable
public class TestableUnitOfMeasureService : NamedEntityService<UnitOfMeasure>
{
    public TestableUnitOfMeasureService(INamedEntityRepository<UnitOfMeasure> repository, IUnitOfWork unitOfWork, ILogger<NamedEntityService<UnitOfMeasure>> logger)
        : base(repository, unitOfWork, logger)
    {
    }
}
