using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Exceptions;

public class UnitOfMeasureTests
{
    private readonly INamedEntityRepository<UnitOfMeasure> _namedEntityRepository;
    private readonly UnitOfMeasureService _unitOfMeasureService;

    public UnitOfMeasureTests()
    {
        _namedEntityRepository = Substitute.For<INamedEntityRepository<UnitOfMeasure>>();
        _unitOfMeasureService = new UnitOfMeasureService(_namedEntityRepository);
    }

    [Fact]
    public async Task CreateUnitOfMeasureWithUniqueNameTest()
    {
        // arrange
        var name = "Kilogram";
        var guid = Guid.NewGuid();
        
        _namedEntityRepository.CreateAsync(Arg.Is<UnitOfMeasure>(u => u.Name == name)).Returns(guid);
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(false);

        // act
        var result = await _unitOfMeasureService.CreateUnitOfMeasureAsync(name);

        // assert
        Assert.Equal(guid, result);
    }

    [Fact]
    public async Task CreateUnitOfMeasureWithSameNameTest()
    {
        // arrange
        var name = "Kilogram";
        
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(true);

        // assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _unitOfMeasureService.CreateUnitOfMeasureAsync(name));
    }

    [Fact]
    public async Task UpdateUnitOfMeasureWithValidDataTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Updated Unit";
        var unitOfMeasure = new UnitOfMeasure("Original Unit");

        _namedEntityRepository.GetByIdAsync(id).Returns(unitOfMeasure);
        _namedEntityRepository.ExistsWithNameAsync(name).Returns(false);
        _namedEntityRepository.UpdateAsync(unitOfMeasure).Returns(true);

        // act
        var result = await _unitOfMeasureService.UpdateUnitOfMeasureAsync(id, name);

        // assert
        Assert.True(result);
        Assert.Equal(name, unitOfMeasure.Name);
    }

    [Fact]
    public async Task UpdateUnitOfMeasureWithNonExistentUnitTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Updated Unit";

        _namedEntityRepository.GetByIdAsync(id).Returns((UnitOfMeasure)null!);

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _unitOfMeasureService.UpdateUnitOfMeasureAsync(id, name));
    }

    [Fact]
    public async Task UpdateUnitOfMeasureWithDuplicateNameTest()
    {
        // arrange
        var id = Guid.NewGuid();
        var name = "Duplicate Unit";
        var unitOfMeasure = new UnitOfMeasure("Original Unit"){Id = id};

        _namedEntityRepository.GetByIdAsync(id).Returns(unitOfMeasure);
        _namedEntityRepository.ExistsWithNameAsync(name, id).Returns(true);

        // act & assert
        await Assert.ThrowsAsync<DuplicateEntityException>(() => _unitOfMeasureService.UpdateUnitOfMeasureAsync(id, name));
    }
}