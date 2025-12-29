using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Tests.Domain.Aggregates.NamedAggregates;

public class ResourceTests
{
    [Fact]
    public void create_should_create_resource_when_valid_name_provided()
    {
        // Arrange
        const string name = "Steel";

        // Act
        var resource = Resource.Create(name);

        // Assert
        resource.Name.Should().Be(name);
        resource.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void create_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => Resource.Create(invalidName);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void rename_should_update_name_when_valid_name_provided()
    {
        // Arrange
        var resource = Resource.Create("Old Name");
        const string newName = "New Name";

        // Act
        resource.Rename(newName);

        // Assert
        resource.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void rename_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Arrange
        var resource = Resource.Create("Valid Name");

        // Act
        var action = () => resource.Rename(invalidName);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void archive_should_set_is_active_to_false()
    {
        // Arrange
        var resource = Resource.Create("Test Resource");

        // Act
        resource.Archive();

        // Assert
        resource.IsActive.Should().BeFalse();
    }

    [Fact]
    public void activate_should_set_is_active_to_true()
    {
        // Arrange
        var resource = Resource.Create("Test Resource");
        resource.Archive();

        // Act
        resource.Activate();

        // Assert
        resource.IsActive.Should().BeTrue();
    }
}
