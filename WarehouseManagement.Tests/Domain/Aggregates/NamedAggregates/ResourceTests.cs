using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.Aggregates.NamedAggregates;

public class ResourceTests
{
    [Fact]
    public void constructor_should_create_resource_when_valid_name_provided()
    {
        // Arrange
        const string name = "Steel";

        // Act
        var resource = new Resource(name);

        // Assert
        resource.Name.Should().Be(name);
        resource.IsActive.Should().BeTrue();
        resource.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void constructor_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => new Resource(invalidName);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void rename_should_update_name_when_valid_name_provided()
    {
        // Arrange
        var resource = new Resource("Old Name");
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
        var resource = new Resource("Valid Name");

        // Act
        var action = () => resource.Rename(invalidName);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void archive_should_set_is_active_to_false()
    {
        // Arrange
        var resource = new Resource("Test Resource");

        // Act
        resource.Archive();

        // Assert
        resource.IsActive.Should().BeFalse();
    }

    [Fact]
    public void activate_should_set_is_active_to_true()
    {
        // Arrange
        var resource = new Resource("Test Resource");
        resource.Archive();

        // Act
        resource.Activate();

        // Assert
        resource.IsActive.Should().BeTrue();
    }
}