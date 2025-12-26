using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.Aggregates.NamedAggregates;

public class ClientTests
{
    [Fact]
    public void constructor_should_create_client_when_valid_data_provided()
    {
        // Arrange
        const string name = "ABC Corp";
        const string address = "123 Main St";

        // Act
        var client = new Client(name, address);

        // Assert
        client.Name.Should().Be(name);
        client.Address.Name.Should().Be(address);
        client.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void constructor_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => new Client(invalidName, "Valid Address");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void constructor_should_throw_exception_when_invalid_address_provided(string invalidAddress)
    {
        // Act
        var action = () => new Client("Valid Name", invalidAddress);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void change_address_should_update_address_when_valid_address_provided()
    {
        // Arrange
        var client = new Client("ABC Corp", "Old Address");
        const string newAddress = "New Address";

        // Act
        client.ChangeAddress(newAddress);

        // Assert
        client.Address.Name.Should().Be(newAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void change_address_should_throw_exception_when_invalid_address_provided(string invalidAddress)
    {
        // Arrange
        var client = new Client("ABC Corp", "Valid Address");

        // Act
        var action = () => client.ChangeAddress(invalidAddress);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void archive_should_set_is_active_to_false()
    {
        // Arrange
        var client = new Client("ABC Corp", "123 Main St");

        // Act
        client.Archive();

        // Assert
        client.IsActive.Should().BeFalse();
    }

    [Fact]
    public void activate_should_set_is_active_to_true()
    {
        // Arrange
        var client = new Client("ABC Corp", "123 Main St");
        client.Archive();

        // Act
        client.Activate();

        // Assert
        client.IsActive.Should().BeTrue();
    }
}