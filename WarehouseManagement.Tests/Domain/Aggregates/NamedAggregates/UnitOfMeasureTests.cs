using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Tests.Domain.Aggregates.NamedAggregates;

public class UnitOfMeasureTests
{
    [Fact]
    public void create_should_create_unit_of_measure_when_valid_name_provided()
    {
        // Arrange
        const string name = "Kilogram";

        // Act
        var unit = UnitOfMeasure.Create(name);

        // Assert
        unit.Name.Should().Be(name);
        unit.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void create_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => UnitOfMeasure.Create(invalidName);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void rename_should_update_name_when_valid_name_provided()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Old Name");
        const string newName = "New Name";

        // Act
        unit.Rename(newName);

        // Assert
        unit.Name.Should().Be(newName);
    }

    [Fact]
    public void archive_should_set_is_active_to_false()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Kilogram");

        // Act
        unit.Archive();

        // Assert
        unit.IsActive.Should().BeFalse();
    }

    [Fact]
    public void activate_should_set_is_active_to_true()
    {
        // Arrange
        var unit = UnitOfMeasure.Create("Kilogram");
        unit.Archive();

        // Act
        unit.Activate();

        // Assert
        unit.IsActive.Should().BeTrue();
    }
}
